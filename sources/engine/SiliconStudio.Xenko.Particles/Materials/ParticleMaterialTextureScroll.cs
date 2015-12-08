﻿using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Internals;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("ParticleMaterialTextureScroll")]
    [Display("Scrolling Texture")]
    public class ParticleMaterialTextureScroll : ParticleMaterialBase
    {
        private Texture texture0 = null;

        [DataMember(100)]
        [Display("Texture")]
        public Texture Texture
        {
            get { return texture0; }
            set
            {
                texture0 = value;
                if (value != null)
                {
                    MandatoryVariation |= ParticleEffectVariation.HasTex0;
                }
                else
                {
                    MandatoryVariation &= ~ParticleEffectVariation.HasTex0;
                }
            }
        }

        [DataMemberIgnore]
        private TextureAddressMode address0 = TextureAddressMode.Wrap;

        [DataMember(102)]
        [Display("Address mode")]
        public TextureAddressMode AddressMode0
        {
            get { return address0; }
            set
            {
                address0 = value;
                dirtySamplerState0 = true;
            }
        }

        [DataMemberIgnore]
        private SamplerState sampler0 = null;

        [DataMemberIgnore]
        private bool dirtySamplerState0 = true;

        [DataMemberIgnore]
        protected uint TextureSwizzle = 0;

        // TODO: Distribution

        [DataMember(110)]
        [Display("Color Min")]
        public Color4 ColorMin { get; set; } = new Color4(1, 1, 1, 1);

        [DataMember(120)]
        [Display("Color Max")]
        public Color4 ColorMax { get; set; } = new Color4(1, 1, 1, 1);


        [DataMember(130)]
        [Display("Color Random Seed")]
        public UInt32 ColorRandomOffset { get; set; } = 1;

        [DataMember(200)]
        [Display("Start frame")]
        public Vector4 StartFrame { get; set; } = new Vector4(0, 0, 1, 1);

        [DataMember(240)]
        [Display("End frame")]
        public Vector4 EndFrame { get; set; } = new Vector4(0, 1, 1, 2);

        public override void Setup(GraphicsDevice graphicsDevice, ParticleEffectVariation variation, Matrix viewMatrix, Matrix projMatrix, Color4 color)
        {
            PrepareEffect(graphicsDevice, variation);

            // This should be CB0 - view/proj matrices don't change per material
            SetParameter(ParticleBaseKeys.MatrixTransform, viewMatrix * projMatrix);

            // Texture swizzle - if the texture is grayscale, sample it like Tex.rrrr rather than Tex.rgba
            TextureSwizzle = (texture0?.Format == PixelFormat.R32_Float ||
                              texture0?.Format == PixelFormat.A8_UNorm ||
                              texture0?.Format == PixelFormat.BC4_UNorm) ? (uint)1 : 0;
            SetParameter(ParticleBaseKeys.RenderFlagSwizzle, TextureSwizzle);


            // If particles don't have individual color, we can pass the color tint as part of the uniform color scale
            SetParameter(ParticleBaseKeys.ColorScaleMin, color * ColorMin);
            SetParameter(ParticleBaseKeys.ColorScaleMax, color * ColorMax);
            SetParameter(ParticleBaseKeys.ColorScaleOffset, ColorRandomOffset);

            if (sampler0 == null || dirtySamplerState0)
            {
                sampler0 = SamplerState.New(graphicsDevice, new SamplerStateDescription(TextureFilter.Linear, address0));
                dirtySamplerState0 = false;
            }
            SetParameter(TexturingKeys.Texture0, texture0);
            SetParameter(TexturingKeys.Sampler0, sampler0);

            ApplyEffect(graphicsDevice);
        }

        // TODO Make some sort of accessor or enumerator around ParticlePool which can also sort particles
        public unsafe override void PatchVertexBuffer(ParticleVertexLayout vtxBuilder, Vector3 invViewX, Vector3 invViewY, int maxVertices, ParticlePool pool, ParticleEmitter emitter = null)
        {
            var vtxPerParticle = vtxBuilder.VerticesPerParticle;
            var numberOfParticles = Math.Min(maxVertices / vtxPerParticle, pool.LivingParticles);
            if (numberOfParticles <= 0)
                return;

            var lifeField = pool.GetField(ParticleFields.RemainingLife);
            var randField = pool.GetField(ParticleFields.RandomSeed);

            if (!randField.IsValid() || !lifeField.IsValid())
                return;

            var lifeMin = emitter?.ParticleMinLifetime ?? 1;
            var lifeMax = emitter?.ParticleMaxLifetime ?? 1;
            var lifeStep = lifeMax - lifeMin;

            var colorField = pool.GetField(ParticleFields.Color);
            var hasColorField = colorField.IsValid();

            var whiteColor = new Color4(1, 1, 1, 1);

            var renderedParticles = 0;

            // TODO Fetch sorted particles
            foreach (var particle in pool)
            {
                vtxBuilder.SetRandomSeedForParticle(particle[randField]);
                var randSeed = *(RandomSeed*)(particle[randField]);

                var remainingLife = *(float*)(particle[lifeField]);
                var startingLife = lifeMin + lifeStep * randSeed.GetFloat(0);
                var normalizedTimeline = 1f - remainingLife / startingLife;
                // vtxBuilder.SetLifetimeForParticle(normalizedTimeline);   // To check - do we need normalized life or absolute?
                vtxBuilder.SetLifetimeForParticle(particle[lifeField]);     // To check - do we need normalized life or absolute?

                vtxBuilder.SetColorForParticle(hasColorField ? particle[colorField] : (IntPtr)(&whiteColor));

                var uvTransform = Vector4.Lerp(StartFrame, EndFrame, normalizedTimeline);
                uvTransform.Z -= uvTransform.X;
                uvTransform.W -= uvTransform.Y;

                for (int i = 0; i < vtxBuilder.VerticesPerParticle; i++)
                {
                    vtxBuilder.TransformUvCoords(ref uvTransform);
                    vtxBuilder.NextVertex();
                }

                maxVertices -= vtxPerParticle;

                if (++renderedParticles >= numberOfParticles)
                {
                    return;
                }
            }


        }

    }
}
