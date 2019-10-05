﻿// NOTE: Based on URP Lighting.hlsl which rplaced some half3 with floats to avoid lighting artifacts on mobile

Shader "Lux LWRP/Water"
{
    Properties
    {
        [Header(Surface Options)]
        [Space(5)]
        [Enum(Off,0,On,1)]_ZWrite       ("ZWrite", Float) = 1.0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4 // "LessEqual"
        // [Enum(UnityEngine.Rendering.CullMode)] _Culling ("Culling", Float) = 0
        [ToggleOff(_RECEIVE_SHADOWS_OFF)]
        _ReceiveShadows                 ("Receive Shadows", Float) = 1.0
        [Toggle(ORTHO_SUPPORT)]
        _OrthoSpport                    ("Enable Orthographic Support", Float) = 0

        [Header(Surface Inputs)]
        [Space(5)]
        _BumpMap                        ("Water Normal Map", 2D) = "bump" {}
        _BumpScale                      ("Normal Scale", Float) = 1.0
        [LuxLWRPVectorTwoDrawer]
        _Speed                          ("Speed (UV)", Vector) = (0.1, 0, 0, 0)
        [LuxLWRPVectorFourDrawer] 
        _SecondaryTilingSpeedRefractBump("Secondary Bump", Vector) = (2, 2.3, 0.1, 1)
        [LuxLWRPHelpDrawer] _Help       ("Tiling (X) Speed (Y) Refraction (Z) Bump Scale (W)", Float) = 1
        [Space(5)]
        _Smoothness                     ("Smoothness", Range(0.0, 1.0)) = 0.5
        _SpecColor                      ("Specular", Color) = (0.2, 0.2, 0.2)
        [Space(5)]
        _EdgeBlend                      ("Edge Blending", Range(0.1, 10.0)) = 2.0 
        _Refraction                     ("Refraction", Range(0, 1)) = .25
        _ReflectionBumpScale            ("Reflection Bump Scale", Range(0.1, 1.0)) = 0.3

        [Header(Underwater Fog)]
        [Space(5)]
        _Color                          ("Fog Color", Color) = (.2,.8,.9,1)
        _Density                        ("Density", Float) = 1.0

        [Header(Foam)]
        [Toggle(_FOAM)] _Foam           ("Enable Foam", Float) = 1.0
        [NoScaleOffset] _FoamMap        ("Foam Albedo (RGB) Mask (A)", 2D) = "bump" {}
        _FoamTiling                     ("Foam Tiling", Float) = 2
        [LuxLWRPVectorTwoDrawer]
        _FoamSpeed                      ("Foam Speed (UV)", Vector) = (0.1, 0, 0, 0)
        _FoamScale                      ("Foam Scale", Float) = 4
        _FoamSoftIntersectionFactor     ("Foam Edge Blending", Range(0.1, 3.0)) = 0.5
        _FoamSlopStrength               ("Foam Slope Strength", Range(0.0, 1.0)) = 0.85
        _FoamSmoothness                 ("Foam Smoothness", Range(0.0, 1.0)) = 0.3

        [Header(Advanced)]
        [Space(5)]
        [ToggleOff] _SpecularHighlights ("Enable Specular Highlights", Float) = 1.0
        [ToggleOff]
        _EnvironmentReflections         ("Environment Reflections", Float) = 1.0

    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "LightweightPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        LOD 100

        Pass
        {
            Tags {"LightMode" = "LightweightForward"}
//          Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZTest [_ZTest]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Lightweight Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature _RECEIVE_SHADOWS_OFF

            #pragma shader_feature_local _FOAM

            #pragma shader_feature_local ORTHO_SUPPORT

// #define _ADDITIONAL_LIGHTS_VERTEX
            
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #define _SPECULAR_SETUP 1
            #define _NORMALMAP 1

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
        //  defines a bunch of helper functions (like lerpwhiteto)
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
        //  defines SurfaceData, textures and the functions Alpha, SampleAlbedoAlpha, SampleNormal, SampleEmission
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/SurfaceInput.hlsl"

            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            struct VertexInput
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD0;
                float2 lightmapUV   : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 positionCS : SV_POSITION;
                float4 uv : TEXCOORD0;                          // xy textccord, zw water

                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

                float3 positionWS               : TEXCOORD2;
                
                #ifdef _NORMALMAP
                    half4 normalWS              : TEXCOORD3;    // xyz: normal, w: viewDir.x
                    half4 tangentWS             : TEXCOORD4;    // xyz: tangent, w: viewDir.y
                    half4 bitangentWS           : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
                #else
                    half3 normalWS              : TEXCOORD3;
                    half3 viewDirWS             : TEXCOORD4;
                #endif

                half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light
                #ifdef _MAIN_LIGHT_SHADOWS
                    float4 shadowCoord          : TEXCOORD7;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float _Alpha;
                half3 _SpecColor;
                half _Smoothness;
                half _EdgeBlend;
                float2 _Speed;
                half _BumpScale;
                float4 _SecondaryTilingSpeedRefractBump;
                half4 _Color;
                half _Density;
                half _FresnelPower;
                half _Refraction;
                half _ReflectionBumpScale;
                half _FoamScale;
                half _FoamTiling;
                half2 _FoamSpeed;
                half _FoamSoftIntersectionFactor;
                half _FoamSlopStrength;
                half _FoamSmoothness;
                float4 _BumpMap_ST;
            CBUFFER_END

        //  Defined in SurfaceInput.hlsl
        //  TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture); float4 _CameraOpaqueTexture_TexelSize;
float4 _CameraOpaqueTexture_ST;
            TEXTURE2D(_FoamMap); SAMPLER(sampler_FoamMap); float4 _FoamMap_TexelSize;
            

            VertexOutput vert (VertexInput input)
            {
                VertexOutput output = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                //o.positionWS = TransformObjectToWorld(input.positionOS.xyz); //  mul(UNITY_MATRIX_M, input.vertex).xyz;
                //o.positionCS = TransformWorldToHClip(o.positionWS.xyz); // TransformObjectToHClip(input.positionOS.xyz);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                #if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
                    output.shadowCoord = GetShadowCoord(vertexInput);
                #endif
                
                #ifdef _NORMALMAP
                    float3 viewDirWS = GetCameraPositionWS() - output.positionWS;
                    output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
                    output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
                    output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
                #else
                    output.normalWS.xyz = NormalizeNormalPerVertex(normalInput.normalWS);
                    output.viewDirWS = GetCameraPositionWS() - o.positionWS;
                #endif

                half fogFactor = ComputeFogFactor(output.positionCS.z);
                half3 vertexLight = VertexLighting(output.positionWS, output.normalWS.xyz);

                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
                output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

                output.uv.xy = TRANSFORM_TEX(input.texcoord, _BumpMap) + _Time.xx * _Speed;

            //  Water
// see: ComputeGrabScreenPos
                float4 screenUV = ComputeScreenPos(output.positionCS);
                output.uv.zw = screenUV.xy; //waterDepth.xx;

                return output;
            }


        //  ------------------------------------------------------------------
        //  Helper functions to handle orthographic / perspective projection  

            inline float GetOrthoDepthFromZBuffer (float rawDepth) {
                #if defined(UNITY_REVERSED_Z)
                    rawDepth = 1.0f - rawDepth;
                #endif
                return lerp(_ProjectionParams.y, _ProjectionParams.z, rawDepth);
            }

            inline float GetProperEyeDepth (float rawDepth) {
                #if defined(ORTHO_SUPPORT)
                    float perspectiveSceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                    float orthoSceneDepth = GetOrthoDepthFromZBuffer(rawDepth);
                    return lerp(perspectiveSceneDepth, orthoSceneDepth, unity_OrthoParams.w);
                #else
                    return LinearEyeDepth(rawDepth, _ZBufferParams);
                #endif
            }


            half4 frag (VertexOutput IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                //half3 albedo = 0;
                //half metallic = 0;
                half3 specular = _SpecColor;
                half smoothness = _Smoothness;
                half occlusion = 1;
                half emission = 0;
                half alpha = 1;
                half3 normalTS = half3(0,0,1);
                half3 refraction = 0;

                float surfaceEyeDepth = GetProperEyeDepth(IN.positionCS.z); // LinearEyeDepth(IN.positionCS.z, _ZBufferParams);

            //  We have to reset i.grabUV.w as otherwise texture projection does not work
                #if defined(ORTHO_SUPPORT)
                    IN.positionCS.w = lerp(IN.positionCS.w, 1.0f, unity_OrthoParams.w);
                #endif

                float2 screenUV = IN.uv.zw / IN.positionCS.w;

//  Fix screenUV for Single Pass Stereo Rendering
    #if defined(UNITY_SINGLE_PASS_STEREO)
        //screenUV.x = screenUV.x * 0.5f + (float)unity_StereoEyeIndex * 0.5f;
        screenUV.xy = UnityStereoTransformScreenSpaceTex(screenUV.xy);
    #endif

                half4 normalSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv.xy);

            //  ////////////
            //  Get the normals
                #if BUMP_SCALE_NOT_SUPPORTED
                    normalTS =  UnpackNormal(normalSample);
                    normalSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv.xy * _SecondaryTilingSpeedRefractBump.x + _Time.xx * _Speed * _SecondaryTilingSpeedRefractBump.y + normalTS.xz * _SecondaryTilingSpeedRefractBump.z );
                    half3 detailNormal = UnpackNormal(normalSample);
                    normalTS = normalize(half3(normalTS.xy + detailNormal.xy, normalTS.z * detailNormal.z)); 
                #else
                    normalTS = UnpackNormalScale(normalSample, _BumpScale);
                    normalSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv.xy * _SecondaryTilingSpeedRefractBump.x + _Time.xx * _Speed * _SecondaryTilingSpeedRefractBump.y + normalTS.xz * _SecondaryTilingSpeedRefractBump.z );
                    half3 detailNormal = UnpackNormalScale(normalSample, _SecondaryTilingSpeedRefractBump.w);
                    normalTS = normalize(half3(normalTS.xy + detailNormal.xy, normalTS.z * detailNormal.z)); 
                #endif
            
            //  World space normal - as we need it for view space normal (skipped)
                half3 normalWS = TransformTangentToWorld(normalTS, half3x3(IN.tangentWS.xyz, IN.bitangentWS.xyz, IN.normalWS.xyz));
                normalWS = NormalizeNormalPerPixel(normalWS);

            //  ////////////
            //  Refraction
            //  Skipped view space normal and went with tangent space instead
                //half3 viewNormal = mul((float3x3)GetWorldToHClipMatrix(), -normalWS).xyz;
                //float2 offset = viewNormal.xz * _Refraction;

                float distanceFadeFactor = IN.positionCS.z * _ZBufferParams.z;

            //  OpenGL Core
                #if UNITY_REVERSED_Z != 1
                    distanceFadeFactor = IN.positionCS.z / IN.positionCS.w;
                #endif

            //  Somehow handle orthographic projection
                #if defined(ORTHO_SUPPORT)
                    distanceFadeFactor = (unity_OrthoParams.w) ? 1.0f / unity_OrthoParams.x : distanceFadeFactor;
                #endif

                float2 offset = normalTS.xy * _Refraction * distanceFadeFactor;

                float refractedSceneDepth = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV + offset, 0);
                refractedSceneDepth = GetProperEyeDepth(refractedSceneDepth); //LinearEyeDepth(refractedSceneDepth, _ZBufferParams);
                float viewDepth = refractedSceneDepth - surfaceEyeDepth;
            
            //  Do not refract pixel of the foreground
                offset = screenUV + offset * saturate(viewDepth);
                refractedSceneDepth = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, offset, 0);
                refraction = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, ( offset ) ).rgb;
                refractedSceneDepth = GetProperEyeDepth(refractedSceneDepth); //LinearEyeDepth(refractedSceneDepth, _ZBufferParams);
                viewDepth = refractedSceneDepth - surfaceEyeDepth;

            //  In case we use HDR refraction may get way too bright.
                refraction = saturate(refraction);

            //  Final blend value
                alpha = saturate ( _EdgeBlend * viewDepth );

            //  ////////////
            //  Underwater fog
            //  Calculate Attenuation along viewDirection
                float viewAtten = saturate( 1.0 - exp( -viewDepth * _Density) );
                float underwaterFogDensity = viewAtten;

            //  ////////////
            //  Foam
                #if defined(_FOAM)
                    half FoamSoftIntersection = saturate( _FoamSoftIntersectionFactor * (viewDepth ));
                    half FoamThreshold = normalTS.z * 2 - 1;
                //  Get shoreline foam mask
                    float shorelineFoam = saturate(-FoamSoftIntersection * (1 + FoamThreshold) + 1 );
                    shorelineFoam = shorelineFoam * saturate(1 * FoamSoftIntersection - FoamSoftIntersection * FoamSoftIntersection );
                    half4 rawFoamSample = SAMPLE_TEXTURE2D(_FoamMap, sampler_FoamMap, IN.uv.xy * _FoamTiling + normalTS.xy * 0.02 + _Time.xx * _FoamSpeed);
                //  Add foam on slopes
                    shorelineFoam += saturate(1 - IN.normalWS.y) * _FoamSlopStrength;
                //  Combine sample and distribution(shorelineFoam)
                    rawFoamSample.a = saturate(rawFoamSample.a * shorelineFoam * _FoamScale  * ( 1 - (normalTS.x + normalTS.y) * 4) );
                //  Errode foam
                    rawFoamSample.a = rawFoamSample.a * smoothstep( 0.8 - rawFoamSample.a, 1.6 - rawFoamSample.a, rawFoamSample.a );
                //  Adjust smoothess to foam
                    smoothness = lerp(smoothness, _FoamSmoothness, rawFoamSample.a);
                #endif


            //  ////////////    
            //  Transfer all to world space and prepare inputData (for convenience)
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;

                #ifdef _NORMALMAP
                    inputData.normalWS = normalWS;
                    inputData.viewDirectionWS = SafeNormalize( float3(IN.normalWS.w, IN.tangentWS.w, IN.bitangentWS.w) );
                #else
                    inputData.normalWS = IN.normalWS.xyz; // no normal map
                    inputData.viewDirectionWS = SafeNormalize(IN.viewDirWS);
                #endif
                
                #if defined(_MAIN_LIGHT_SHADOWS)
                    #if SHADOWS_SCREEN
                    //  Refract shadows / * IN.positionCS.w because unity will divide
                        inputData.shadowCoord = float4(offset * IN.positionCS.w, IN.shadowCoord.zw);
                    #else
                        inputData.shadowCoord = IN.shadowCoord;
                    #endif
                #else
                    inputData.shadowCoord = float4(0, 0, 0, 0);
                #endif

//  Fix shadowCoord for Single Pass Stereo Rendering
    #if defined(UNITY_SINGLE_PASS_STEREO)
        #if SHADOWS_SCREEN
        //  Shadows perform : UnityStereoTransformScreenSpaceTex(shadowCoord.xy) after perspective division;
        //  We do it manually:        
            inputData.shadowCoord.xy =  inputData.shadowCoord.xy / inputData.shadowCoord.w;
            inputData.shadowCoord.w = 1.0f;
        //  inputData.shadowCoord.x = screenUV.x;
        //  Then we reset shadowCoord.w and unity_StereoScaleOffset so it does not get applied twice
            unity_StereoScaleOffset[0] = float4(1,1,0,0);
            unity_StereoScaleOffset[1] = float4(1,1,0,0);
        #endif
    #endif


                inputData.fogCoord = IN.fogFactorAndVertexLight.x;
                inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
                inputData.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, inputData.normalWS);

            //  /////////
            //  Apply lighting
                half4 color = 1;
                half3 origRefraction = refraction;

            //  Get fog
                real fogFactor = IN.fogFactorAndVertexLight.x;
                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                    #if defined(FOG_EXP)
                        fogFactor = saturate(exp2(-fogFactor));
                    #elif defined(FOG_EXP2)
                        fogFactor = saturate(exp2(-fogFactor*fogFactor));
                    #endif
                #endif

            //  Prepare missing Inputs
                half reflectivity = ReflectivitySpecular(specular);
                half oneMinusReflectivity = 1.0 - reflectivity;
                half perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(smoothness);
                half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
                half roughness2 = roughness * roughness;
                half normalizationTerm = roughness * 4.0h + 2.0h;

            //  Get light
                Light mainLight = GetMainLight(inputData.shadowCoord);
                half3 lightColorAndAttenuation = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);

half3 shad =  lightColorAndAttenuation;               

                MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

                #ifdef _ADDITIONAL_LIGHTS
                    int pixelLightCount = GetAdditionalLightsCount();
                #endif

                half NdotL = saturate(dot( inputData.normalWS, mainLight.direction));
                half3 VertexAndGILighting = IN.fogFactorAndVertexLight.yzw + inputData.bakedGI;

            //  Diffuse underwater lighting
                half diffuse_nl = saturate(dot(half3(0,1,0), mainLight.direction));
                //half3 diffuseUnderwaterLighting = _Color * (lightColorAndAttenuation * diffuse_nl + VertexAndGILighting);
            //  Shadows are sampled at the bottom surface. So we attenuate them by underwaterFogDensity. Just a hack but it looks better than not doing anything here.
                half3 diffuseUnderwaterLighting = _Color.rgb * (VertexAndGILighting + (diffuse_nl * 
                    mainLight.color * mainLight.distanceAttenuation * 
                    lerp( mainLight.shadowAttenuation, 1 , underwaterFogDensity )
                    )
                );

            //  Add foam
                #if defined(_FOAM)
                    half3 foamLighting = rawFoamSample.rgb * (lightColorAndAttenuation * NdotL + VertexAndGILighting);
                #endif
            //  Specular Lighting
                half3 specularLighting = 0;
                #if !defined(_SPECULARHIGHLIGHTS_OFF)
                    float3 halfDir = SafeNormalize(float3(mainLight.direction) + float3(inputData.viewDirectionWS));
                    float NoH = saturate(dot(inputData.normalWS, halfDir));
                    half LoH = saturate(dot(mainLight.direction, halfDir));
                    float d = NoH * NoH * (roughness2 - 1.h) + 1.0001f;
                    half LoH2 = LoH * LoH;
                    half specularTerm = roughness2 / ((d * d) * max(0.1h, LoH2) * normalizationTerm );
                    #if defined (SHADER_API_MOBILE)
                        specularTerm = specularTerm - HALF_MIN;
                        specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
                    #endif
                    specularLighting = specularTerm * specular * lightColorAndAttenuation;
                    specularLighting *= NdotL;
                #endif
                
                #ifdef _ADDITIONAL_LIGHTS
                    for (int i = 0; i < pixelLightCount; ++i) {
                        Light light = GetAdditionalLight(i, inputData.positionWS);
                        NdotL = saturate(dot(inputData.normalWS, light.direction));
                        diffuse_nl = saturate(dot(half3(0,1,0), light.direction));
                        
                        half3 addLightColorAndAttenuation = light.color * light.distanceAttenuation * light.shadowAttenuation;
                        
                        diffuseUnderwaterLighting += _Color.rgb * addLightColorAndAttenuation * diffuse_nl;
                        #if defined(_FOAM)
                            foamLighting += rawFoamSample.rgb * addLightColorAndAttenuation * NdotL;
                        #endif

                        #if !defined(_SPECULARHIGHLIGHTS_OFF)
                            halfDir = SafeNormalize(float3(light.direction) + float3(inputData.viewDirectionWS));
                            NoH = saturate(dot(inputData.normalWS, halfDir));
                            LoH = saturate(dot(light.direction, halfDir));
                            d = NoH * NoH * (roughness2 - 1.h) + 1.0001f;
                            LoH2 = LoH * LoH;
                            specularTerm = roughness2 / ((d * d) * max(0.1h, LoH2) * normalizationTerm );
                            #if defined (SHADER_API_MOBILE)
                                specularTerm = specularTerm - HALF_MIN;
                                specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
                            #endif
                            specularLighting += specularTerm * specular * addLightColorAndAttenuation;
                        #endif
                    }
                #endif

            //  Fog - diffuseUnderwaterLighting
                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                    diffuseUnderwaterLighting = lerp(unity_FogColor.rgb, diffuseUnderwaterLighting, fogFactor);
                #endif
            
            //  Add underwater fog
                refraction.rgb = lerp(refraction.rgb, diffuseUnderwaterLighting, underwaterFogDensity);
                
            //  Fog – foam
                #if defined(_FOAM)
                    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                        foamLighting = lerp(unity_FogColor.rgb, foamLighting, fogFactor);
                    #endif
                #endif

            //  Reflections
                #if !defined(_ENVIRONMENTREFLECTIONS_OFF)
                //  Calculate smoothedReflectionNormal
                    half3 reflectionNormal = lerp( IN.normalWS.xyz, inputData.normalWS, _ReflectionBumpScale);
                    half3 reflectionVector = reflect(-inputData.viewDirectionWS, reflectionNormal);
                    half fresnelTerm = Pow4(1.0 - saturate(dot(inputData.normalWS, inputData.viewDirectionWS)));
                    half3 reflections = GlossyEnvironmentReflection(reflectionVector, perceptualRoughness, occlusion);
                    float surfaceReduction = 1.0 / (roughness2 + 1.0);
                    half grazingTerm = saturate(smoothness + reflectivity);
                    reflections = reflections * surfaceReduction * lerp(specular, grazingTerm, fresnelTerm);
                //  Combine specular lighting and reflections 
                    specularLighting += reflections;
                #endif

                #if !defined(_SPECULARHIGHLIGHTS_OFF) || !defined(_ENVIRONMENTREFLECTIONS_OFF)
                    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                    //  "Apply" fog
                        specularLighting *= fogFactor;
                    #endif
                #endif

            //  Combine all
                color.rgb = refraction.rgb;
                #if defined(_FOAM)            
                    color.rgb = lerp(color.rgb, foamLighting, rawFoamSample.a );
                #endif
                #if !defined(_SPECULARHIGHLIGHTS_OFF) || !defined(_ENVIRONMENTREFLECTIONS_OFF)
                    color.rgb += specularLighting;
                #endif

            //  Soft edge blending
                color.rgb = lerp(origRefraction, color.rgb, alpha.xxx );


//color.rgb = shad;

                return color;
            }
            ENDHLSL
        }
    }
}
