﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;

/*
 * Adapted for the Fog of War feature in SRP
 * ------------------------------------------------------------------------------------------------------------------------
 * Blit Renderer Feature                                                https://github.com/Cyanilux/URP_BlitRenderFeature
 * ------------------------------------------------------------------------------------------------------------------------
 * Based on the Blit from the UniversalRenderingExamples
 * https://github.com/Unity-Technologies/UniversalRenderingExamples/tree/master/Assets/Scripts/Runtime/RenderPasses
 * 
 * Extended to allow for :
 * - Specific access to selecting a source and destination (via current camera's color / texture id / render texture object
 * - Automatic switching to using _AfterPostProcessTexture for After Rendering event, in order to correctly handle the blit after post processing is applied
 * - Setting a _InverseView matrix (cameraToWorldMatrix), for shaders that might need it to handle calculations from screen space to world.
 *     e.g. reconstruct world pos from depth : https://twitter.com/Cyanilux/status/1269353975058501636 
 * ------------------------------------------------------------------------------------------------------------------------
 * @Cyanilux
*/
namespace Edgar.Unity
{
    /// <summary>
    /// Scriptable renderer feature that has to be enabled to make the Fog of War work in LWRP.
    /// </summary>
    public class FogOfWarLWRPFeature : ScriptableRendererFeature
    {

        internal class BlitPass : ScriptableRenderPass
        {

            public Material blitMaterial = null;
            public FilterMode filterMode { get; set; }

            private BlitSettings settings;

            private RenderTargetIdentifier source { get; set; }
            private RenderTargetIdentifier destination { get; set; }

            RenderTargetHandle m_TemporaryColorTexture;
            RenderTargetHandle m_DestinationTexture;
            string m_ProfilerTag;

            public BlitPass(RenderPassEvent renderPassEvent, BlitSettings settings, string tag)
            {
                this.renderPassEvent = renderPassEvent;
                this.settings = settings;
                blitMaterial = settings.blitMaterial;
                m_ProfilerTag = tag;
                m_TemporaryColorTexture.Init("_TemporaryColorTexture");
                if (settings.dstType == Target.TextureID)
                {
                    m_DestinationTexture.Init(settings.dstTextureId);
                }
            }

            public void Setup(RenderTargetIdentifier source, RenderTargetIdentifier destination)
            {
                this.source = source;
                this.destination = destination;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

                RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDesc.depthBufferBits = 0;

                if (settings.setInverseViewMatrix)
                {
                    Shader.SetGlobalMatrix("_InverseView", renderingData.cameraData.camera.cameraToWorldMatrix);
                }

                if (settings.dstType == Target.TextureID)
                {
                    cmd.GetTemporaryRT(m_DestinationTexture.id, opaqueDesc, filterMode);
                }

                //Debug.Log($"src = {source},     dst = {destination} ");
                // Can't read and write to same color target, use a TemporaryRT
                if (source == destination || (settings.srcType == settings.dstType && settings.srcType == Target.CameraColor))
                {
                    cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);
                    Blit(cmd, source, m_TemporaryColorTexture.Identifier(), blitMaterial, settings.blitMaterialPassIndex);
                    Blit(cmd, m_TemporaryColorTexture.Identifier(), destination);
                }
                else
                {
                    Blit(cmd, source, destination, blitMaterial, settings.blitMaterialPassIndex);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                if (settings.dstType == Target.TextureID)
                {
                    cmd.ReleaseTemporaryRT(m_DestinationTexture.id);
                }
                if (source == destination || (settings.srcType == settings.dstType && settings.srcType == Target.CameraColor))
                {
                    cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
                }
            }
        }

        [System.Serializable]
        internal class BlitSettings
        {

            public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;

            [HideInInspector]
            public Material blitMaterial = null;

            [HideInInspector]
            public int blitMaterialPassIndex = 0;

            [HideInInspector]
            public bool setInverseViewMatrix = false;

            [HideInInspector]
            public Target srcType = Target.CameraColor;

            [HideInInspector]
            public string srcTextureId = "_CameraColorTexture";
            
            #pragma warning disable 0649

            [HideInInspector]
            public RenderTexture srcTextureObject;

            #pragma warning restore 0649

            [HideInInspector]
            public Target dstType = Target.CameraColor;

            [HideInInspector]
            public string dstTextureId = "_BlitPassTexture";

            #pragma warning disable 0649

            [HideInInspector]
            public RenderTexture dstTextureObject;

            #pragma warning restore 0649
        }

        internal enum Target
        {
            CameraColor,
            TextureID,
            RenderTextureObject
        }

        [SerializeField]
        internal BlitSettings settings = new BlitSettings();

        BlitPass blitPass;

        private RenderTargetIdentifier srcIdentifier, dstIdentifier;

        public override void Create()
        {
            var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
            settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
            blitPass = new BlitPass(settings.Event, settings, name);

            if (settings.Event == RenderPassEvent.AfterRenderingPostProcessing)
            {
                Debug.LogWarning("Note that the \"After Rendering Post Processing\"'s Color target doesn't seem to work? (or might work, but doesn't contain the post processing) :( -- Use \"After Rendering\" instead!");
            }

            UpdateSrcIdentifier();
            UpdateDstIdentifier();
        }

        private void UpdateSrcIdentifier()
        {
            srcIdentifier = UpdateIdentifier(settings.srcType, settings.srcTextureId, settings.srcTextureObject);
        }

        private void UpdateDstIdentifier()
        {
            dstIdentifier = UpdateIdentifier(settings.dstType, settings.dstTextureId, settings.dstTextureObject);
        }

        private RenderTargetIdentifier UpdateIdentifier(Target type, string s, RenderTexture obj)
        {
            if (type == Target.RenderTextureObject)
            {
                return obj;
            }
            else if (type == Target.TextureID)
            {
                //RenderTargetHandle m_RTHandle = new RenderTargetHandle();
                //m_RTHandle.Init(s);
                //return m_RTHandle.Identifier();
                return s;
            }
            return new RenderTargetIdentifier();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Do not execute the effect in Editor
            if (!Application.isPlaying)
            {
                return;
            }

            // Do not execute the effect if there is no instance of the script
            if (FogOfWar.Instance == null)
            {
                return;
            }

            // Do not execute the effect if the FogOfWar instance is disabled
            if (!FogOfWar.Instance.enabled)
            {
                return;
            }

            // Do not execute the effect if the current camera does not have the FogOfWar component attached
            if (renderingData.cameraData.camera.GetComponent<FogOfWarGrid2D>() == null && renderingData.cameraData.camera.GetComponent<FogOfWarAdditionalCameraGrid2D>() == null)            {
                return;
            }

            var material = FogOfWar.Instance.GetMaterial(renderingData.cameraData.camera);

            if (material == null)
            {
                return;
            }
                
            blitPass.blitMaterial = material;

            if (settings.Event == RenderPassEvent.AfterRenderingPostProcessing)
            {
            }
            // Uncomment for URP
            else if (false /*settings.Event == RenderPassEvent.AfterRendering && renderingData.postProcessingEnabled*/)
            {
                // If event is AfterRendering, and src/dst is using CameraColor, switch to _AfterPostProcessTexture instead.
                //if (settings.srcType == Target.CameraColor)
                //{
                //    settings.srcType = Target.TextureID;
                //    settings.srcTextureId = "_AfterPostProcessTexture";
                //    UpdateSrcIdentifier();
                //}
                //if (settings.dstType == Target.CameraColor)
                //{
                //    settings.dstType = Target.TextureID;
                //    settings.dstTextureId = "_AfterPostProcessTexture";
                //    UpdateDstIdentifier();
                //}
            }
            else
            {
                // If src/dst is using _AfterPostProcessTexture, switch back to CameraColor
                if (settings.srcType == Target.TextureID && settings.srcTextureId == "_AfterPostProcessTexture")
                {
                    settings.srcType = Target.CameraColor;
                    settings.srcTextureId = "";
                    UpdateSrcIdentifier();
                }
                if (settings.dstType == Target.TextureID && settings.dstTextureId == "_AfterPostProcessTexture")
                {
                    settings.dstType = Target.CameraColor;
                    settings.dstTextureId = "";
                    UpdateDstIdentifier();
                }
            }

            var src = (settings.srcType == Target.CameraColor) ? renderer.cameraColorTarget : srcIdentifier;
            var dest = (settings.dstType == Target.CameraColor) ? renderer.cameraColorTarget : dstIdentifier;

            blitPass.Setup(src, dest);
            renderer.EnqueuePass(blitPass);
        }
    }
}