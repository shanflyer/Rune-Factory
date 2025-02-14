Shader "Shader Graphs/URP_SoftNoise"
{
    Properties
    {
        _Numberofwaves("Number of waves", Float) = 1
        _WavesspeedsizeXYTwistspeedsizeZW("Waves speed-size XY Twist speed-size ZW", Vector) = (-1, 0.2, 4, 0.6)
        _VertexScale("VertexScale", Float) = 1
        _MainTex("MainTex", 2D) = "white" {}
        _Noise("Noise", 2D) = "white" {}
        _SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0, 0, 0, 0)
        _Noisescale("Noise scale", Float) = 1000
        _Noisepower("Noise power", Float) = 1
        _Noiselerp("Noise lerp", Float) = 1
        [HDR]_Color("Color", Color) = (1, 1, 1, 1)
        _Emissionpower("Emission power", Float) = 1
        _Emission("Emission", Float) = 1
        _OpacityTex("OpacityTex", 2D) = "white" {}
        _Mask("Mask", 2D) = "white" {}
        _Maskpower("Mask power", Float) = 1
        _Maskmultiplayer("Mask multiplayer", Float) = 3
        [ToggleUI]_Softedges("Soft edges", Float) = 0
        [ToggleUI]_Usedepth("Use depth?", Float) = 0
        _Depthpower("Depth power", Float) = 1
        _OpacityTexspeedXY("OpacityTex Speed XY", Vector) = (0, -0.5, 0, 0)
        _Sideopacitymult("Side opacity mult", Float) = 1.5
        [ToggleUI]_Upopacity("Up opacity", Float) = 1
        [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
            "DisableBatching"="False"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalUnlitSubTarget"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                // LightMode: <None>
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
        #pragma shader_feature _ _SAMPLE_GI
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_UNLIT
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 WorldSpaceNormal;
             float3 ObjectSpaceViewDirection;
             float3 WorldSpaceViewDirection;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float3 positionWS : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _Numberofwaves;
        float4 _WavesspeedsizeXYTwistspeedsizeZW;
        float _VertexScale;
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
        float4 _Noise_TexelSize;
        float4 _Noise_ST;
        float4 _SpeedMainTexUVNoiseZW;
        float _Noisescale;
        float _Noisepower;
        float _Noiselerp;
        float4 _Color;
        float _Emissionpower;
        float _Emission;
        float4 _OpacityTex_TexelSize;
        float4 _OpacityTex_ST;
        float4 _Mask_TexelSize;
        float4 _Mask_ST;
        float _Maskpower;
        float _Maskmultiplayer;
        float _Softedges;
        float _Usedepth;
        float _Depthpower;
        float4 _OpacityTexspeedXY;
        float _Sideopacitymult;
        float _Upopacity;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_Noise);
        SAMPLER(sampler_Noise);
        TEXTURE2D(_OpacityTex);
        SAMPLER(sampler_OpacityTex);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        float2 Unity_Voronoi_RandomVector_LegacySine_float (float2 UV, float offset)
        {
            Hash_LegacySine_2_2_float(UV, UV);
            return float2(sin(UV.y * offset), cos(UV.x * offset)) * 0.5 + 0.5;
        }
        
        void Unity_Voronoi_LegacySine_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    float2 lattice = float2(x, y);
                    float2 offset = Unity_Voronoi_RandomVector_LegacySine_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);
                    if (d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Sign_float(float In, out float Out)
        {
            Out = sign(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_44f0c1ab07e6a485baa8263505d3561c_Out_0_Float = _Noisepower;
            float _Property_e671dda5b7d5728d8daa9041adfc7ba8_Out_0_Float = _Noisescale;
            float _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Out_3_Float;
            float _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Cells_4_Float;
            Unity_Voronoi_LegacySine_float(IN.uv0.xy, float(100), _Property_e671dda5b7d5728d8daa9041adfc7ba8_Out_0_Float, _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Out_3_Float, _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Cells_4_Float);
            float _Multiply_9c8955b400df968fa3d38a4502565377_Out_2_Float;
            Unity_Multiply_float_float(_Property_44f0c1ab07e6a485baa8263505d3561c_Out_0_Float, _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Out_3_Float, _Multiply_9c8955b400df968fa3d38a4502565377_Out_2_Float);
            UnityTexture2D _Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float4 _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4 = _SpeedMainTexUVNoiseZW;
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_R_1_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[0];
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_G_2_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[1];
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_B_3_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[2];
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_A_4_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[3];
            float2 _Vector2_00da01662a67968f944c0070caa8dc55_Out_0_Vector2 = float2(_Split_e997a8b001c94f8c89b40fb8ed45334a_R_1_Float, _Split_e997a8b001c94f8c89b40fb8ed45334a_G_2_Float);
            float2 _Multiply_af92727bf284c08c9960c739abde2c0f_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Vector2_00da01662a67968f944c0070caa8dc55_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_af92727bf284c08c9960c739abde2c0f_Out_2_Vector2);
            float2 _TilingAndOffset_785428c855ac1383a0f7650b556ad228_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_af92727bf284c08c9960c739abde2c0f_Out_2_Vector2, _TilingAndOffset_785428c855ac1383a0f7650b556ad228_Out_3_Vector2);
            float4 _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D.tex, _Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D.samplerstate, _Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_785428c855ac1383a0f7650b556ad228_Out_3_Vector2) );
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_R_4_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.r;
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_G_5_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.g;
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_B_6_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.b;
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_A_7_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.a;
            UnityTexture2D _Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D = UnityBuildTexture2DStruct(_Noise);
            float2 _Vector2_912722ece3bcfb8c9b66aa407cd25dc6_Out_0_Vector2 = float2(_Split_e997a8b001c94f8c89b40fb8ed45334a_B_3_Float, _Split_e997a8b001c94f8c89b40fb8ed45334a_A_4_Float);
            float2 _Multiply_a7450d1dc073518d8474ac32f7624bf9_Out_2_Vector2;
            Unity_Multiply_float2_float2((IN.TimeParameters.x.xx), _Vector2_912722ece3bcfb8c9b66aa407cd25dc6_Out_0_Vector2, _Multiply_a7450d1dc073518d8474ac32f7624bf9_Out_2_Vector2);
            float2 _TilingAndOffset_1b734d5536df74898bf84054510831de_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_a7450d1dc073518d8474ac32f7624bf9_Out_2_Vector2, _TilingAndOffset_1b734d5536df74898bf84054510831de_Out_3_Vector2);
            float4 _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D.tex, _Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D.samplerstate, _Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_1b734d5536df74898bf84054510831de_Out_3_Vector2) );
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_R_4_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.r;
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_G_5_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.g;
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_B_6_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.b;
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_A_7_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.a;
            float4 _Multiply_f2ca1f6ea7179e8aa89b1e0b99e5aa31_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4, _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4, _Multiply_f2ca1f6ea7179e8aa89b1e0b99e5aa31_Out_2_Vector4);
            float _Property_545e979a9125fa83bf79f3f29f090265_Out_0_Float = _Noiselerp;
            float4 _Lerp_9a69789c96a6db8c847dc8ffb41468bc_Out_3_Vector4;
            Unity_Lerp_float4((_Multiply_9c8955b400df968fa3d38a4502565377_Out_2_Float.xxxx), _Multiply_f2ca1f6ea7179e8aa89b1e0b99e5aa31_Out_2_Vector4, (_Property_545e979a9125fa83bf79f3f29f090265_Out_0_Float.xxxx), _Lerp_9a69789c96a6db8c847dc8ffb41468bc_Out_3_Vector4);
            float4 _Absolute_3b487cec1b9a518ca94a075f77aaefe7_Out_1_Vector4;
            Unity_Absolute_float4(_Lerp_9a69789c96a6db8c847dc8ffb41468bc_Out_3_Vector4, _Absolute_3b487cec1b9a518ca94a075f77aaefe7_Out_1_Vector4);
            float _Property_0fd4bfc5c1229a8ca2b6b9c4986a9e64_Out_0_Float = _Emissionpower;
            float4 _Power_b9467dec8254ce8a963315301f8da311_Out_2_Vector4;
            Unity_Power_float4(_Absolute_3b487cec1b9a518ca94a075f77aaefe7_Out_1_Vector4, (_Property_0fd4bfc5c1229a8ca2b6b9c4986a9e64_Out_0_Float.xxxx), _Power_b9467dec8254ce8a963315301f8da311_Out_2_Vector4);
            float _Property_21bbaf00d2ccc38698a609ad485f67fa_Out_0_Float = _Emission;
            float4 _Multiply_b54d9efacf2c88829e5dfc4a0ffe1837_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Power_b9467dec8254ce8a963315301f8da311_Out_2_Vector4, (_Property_21bbaf00d2ccc38698a609ad485f67fa_Out_0_Float.xxxx), _Multiply_b54d9efacf2c88829e5dfc4a0ffe1837_Out_2_Vector4);
            float4 _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
            float4 _Multiply_df32f698e120fb8f822b54c5fab3878f_Out_2_Vector4;
            Unity_Multiply_float4_float4(IN.VertexColor, _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4, _Multiply_df32f698e120fb8f822b54c5fab3878f_Out_2_Vector4);
            float4 _Multiply_d562aca368957f83b86107a4542c7189_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_b54d9efacf2c88829e5dfc4a0ffe1837_Out_2_Vector4, _Multiply_df32f698e120fb8f822b54c5fab3878f_Out_2_Vector4, _Multiply_d562aca368957f83b86107a4542c7189_Out_2_Vector4);
            float _Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean = _Softedges;
            float _Split_08b2a371dca50b88abcbf5c2e7362375_R_1_Float = IN.VertexColor[0];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_G_2_Float = IN.VertexColor[1];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_B_3_Float = IN.VertexColor[2];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float = IN.VertexColor[3];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_R_1_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[0];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_G_2_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[1];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_B_3_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[2];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[3];
            float _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float;
            Unity_Multiply_float_float(_Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float, _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float, _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float);
            UnityTexture2D _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D = UnityBuildTexture2DStruct(_OpacityTex);
            float4 _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4 = _OpacityTexspeedXY;
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[0];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[1];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_B_3_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[2];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_A_4_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[3];
            float2 _Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2 = float2(_Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float, _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float);
            float2 _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2);
            float2 _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2, _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2);
            float4 _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.tex, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.samplerstate, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2) );
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.r;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_G_5_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.g;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_B_6_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.b;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_A_7_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.a;
            float _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float;
            Unity_Absolute_float(_SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float, _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float);
            float _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float = _Maskpower;
            float _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float;
            Unity_Power_float(_Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float, _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float, _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float);
            float _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float = _Maskmultiplayer;
            float _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float;
            Unity_Multiply_float_float(_Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float, _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float, _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float);
            float _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float;
            Unity_Clamp_float(_Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float, float(0), float(1), _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float);
            UnityTexture2D _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D = UnityBuildTexture2DStruct(_Mask);
            float4 _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.tex, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.samplerstate, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_R_4_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.r;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_G_5_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.g;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_B_6_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.b;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.a;
            float _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float;
            Unity_Multiply_float_float(_Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float, _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float);
            float _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float);
            float _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float;
            Unity_DotProduct_float3(IN.ObjectSpaceNormal, IN.ObjectSpaceViewDirection, _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float);
            float _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float;
            Unity_Absolute_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float);
            float _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float;
            Unity_Power_float(_Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float, float(3), _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float);
            float _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float = _Sideopacitymult;
            float _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float;
            Unity_Multiply_float_float(_Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float, _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float, _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float);
            float _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float;
            Unity_Clamp_float(_Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float, float(0), float(1), _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float);
            float _Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean = _Upopacity;
            float4 _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4 = IN.uv0;
            float _Split_50de07629cf17787826e85912d0e76d2_R_1_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[0];
            float _Split_50de07629cf17787826e85912d0e76d2_G_2_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[1];
            float _Split_50de07629cf17787826e85912d0e76d2_B_3_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[2];
            float _Split_50de07629cf17787826e85912d0e76d2_A_4_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[3];
            float _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float;
            Unity_Absolute_float(_Split_50de07629cf17787826e85912d0e76d2_G_2_Float, _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float);
            float _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float;
            Unity_Power_float(_Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float, float(4), _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float);
            float _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float;
            Unity_Multiply_float_float(_Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float, 3, _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float);
            float _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float;
            Unity_Clamp_float(_Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float, float(0), float(1), _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float);
            float _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float;
            Unity_Branch_float(_Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean, _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float, float(1), _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float);
            float _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float;
            Unity_Remap_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, float2 (0, -1), float2 (0, 1), _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float);
            float _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float;
            Unity_Multiply_float_float(_Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float, _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float);
            float _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float;
            Unity_Sign_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float);
            float _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float;
            Unity_Remap_float(_Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float, float2 (-1, 1), float2 (1, 0), _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float);
            float _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float;
            Unity_Lerp_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float, _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float, _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float);
            float _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float;
            Unity_Clamp_float(_Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float, float(0), float(1), _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float);
            float _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float);
            float _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            Unity_Branch_float(_Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float);
            surface.BaseColor = (_Multiply_d562aca368957f83b86107a4542c7189_Out_2_Vector4.xyz);
            surface.Alpha = _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.ObjectSpaceNormal = normalize(mul(output.WorldSpaceNormal, (float3x3) UNITY_MATRIX_M));           // transposed multiplication by inverse matrix to handle normal scale
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.ObjectSpaceViewDirection = TransformWorldToObjectDir(output.WorldSpaceViewDirection);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Universal Forward"
           Tags { "LightMode" = "ObjDepth" }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
        #pragma shader_feature _ _SAMPLE_GI
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_UNLIT
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 WorldSpaceNormal;
             float3 ObjectSpaceViewDirection;
             float3 WorldSpaceViewDirection;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float3 positionWS : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _Numberofwaves;
        float4 _WavesspeedsizeXYTwistspeedsizeZW;
        float _VertexScale;
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
        float4 _Noise_TexelSize;
        float4 _Noise_ST;
        float4 _SpeedMainTexUVNoiseZW;
        float _Noisescale;
        float _Noisepower;
        float _Noiselerp;
        float4 _Color;
        float _Emissionpower;
        float _Emission;
        float4 _OpacityTex_TexelSize;
        float4 _OpacityTex_ST;
        float4 _Mask_TexelSize;
        float4 _Mask_ST;
        float _Maskpower;
        float _Maskmultiplayer;
        float _Softedges;
        float _Usedepth;
        float _Depthpower;
        float4 _OpacityTexspeedXY;
        float _Sideopacitymult;
        float _Upopacity;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_Noise);
        SAMPLER(sampler_Noise);
        TEXTURE2D(_OpacityTex);
        SAMPLER(sampler_OpacityTex);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        float2 Unity_Voronoi_RandomVector_LegacySine_float (float2 UV, float offset)
        {
            Hash_LegacySine_2_2_float(UV, UV);
            return float2(sin(UV.y * offset), cos(UV.x * offset)) * 0.5 + 0.5;
        }
        
        void Unity_Voronoi_LegacySine_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    float2 lattice = float2(x, y);
                    float2 offset = Unity_Voronoi_RandomVector_LegacySine_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);
                    if (d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Sign_float(float In, out float Out)
        {
            Out = sign(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_44f0c1ab07e6a485baa8263505d3561c_Out_0_Float = _Noisepower;
            float _Property_e671dda5b7d5728d8daa9041adfc7ba8_Out_0_Float = _Noisescale;
            float _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Out_3_Float;
            float _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Cells_4_Float;
            Unity_Voronoi_LegacySine_float(IN.uv0.xy, float(100), _Property_e671dda5b7d5728d8daa9041adfc7ba8_Out_0_Float, _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Out_3_Float, _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Cells_4_Float);
            float _Multiply_9c8955b400df968fa3d38a4502565377_Out_2_Float;
            Unity_Multiply_float_float(_Property_44f0c1ab07e6a485baa8263505d3561c_Out_0_Float, _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Out_3_Float, _Multiply_9c8955b400df968fa3d38a4502565377_Out_2_Float);
            UnityTexture2D _Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float4 _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4 = _SpeedMainTexUVNoiseZW;
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_R_1_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[0];
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_G_2_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[1];
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_B_3_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[2];
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_A_4_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[3];
            float2 _Vector2_00da01662a67968f944c0070caa8dc55_Out_0_Vector2 = float2(_Split_e997a8b001c94f8c89b40fb8ed45334a_R_1_Float, _Split_e997a8b001c94f8c89b40fb8ed45334a_G_2_Float);
            float2 _Multiply_af92727bf284c08c9960c739abde2c0f_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Vector2_00da01662a67968f944c0070caa8dc55_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_af92727bf284c08c9960c739abde2c0f_Out_2_Vector2);
            float2 _TilingAndOffset_785428c855ac1383a0f7650b556ad228_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_af92727bf284c08c9960c739abde2c0f_Out_2_Vector2, _TilingAndOffset_785428c855ac1383a0f7650b556ad228_Out_3_Vector2);
            float4 _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D.tex, _Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D.samplerstate, _Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_785428c855ac1383a0f7650b556ad228_Out_3_Vector2) );
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_R_4_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.r;
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_G_5_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.g;
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_B_6_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.b;
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_A_7_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.a;
            UnityTexture2D _Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D = UnityBuildTexture2DStruct(_Noise);
            float2 _Vector2_912722ece3bcfb8c9b66aa407cd25dc6_Out_0_Vector2 = float2(_Split_e997a8b001c94f8c89b40fb8ed45334a_B_3_Float, _Split_e997a8b001c94f8c89b40fb8ed45334a_A_4_Float);
            float2 _Multiply_a7450d1dc073518d8474ac32f7624bf9_Out_2_Vector2;
            Unity_Multiply_float2_float2((IN.TimeParameters.x.xx), _Vector2_912722ece3bcfb8c9b66aa407cd25dc6_Out_0_Vector2, _Multiply_a7450d1dc073518d8474ac32f7624bf9_Out_2_Vector2);
            float2 _TilingAndOffset_1b734d5536df74898bf84054510831de_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_a7450d1dc073518d8474ac32f7624bf9_Out_2_Vector2, _TilingAndOffset_1b734d5536df74898bf84054510831de_Out_3_Vector2);
            float4 _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D.tex, _Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D.samplerstate, _Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_1b734d5536df74898bf84054510831de_Out_3_Vector2) );
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_R_4_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.r;
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_G_5_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.g;
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_B_6_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.b;
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_A_7_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.a;
            float4 _Multiply_f2ca1f6ea7179e8aa89b1e0b99e5aa31_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4, _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4, _Multiply_f2ca1f6ea7179e8aa89b1e0b99e5aa31_Out_2_Vector4);
            float _Property_545e979a9125fa83bf79f3f29f090265_Out_0_Float = _Noiselerp;
            float4 _Lerp_9a69789c96a6db8c847dc8ffb41468bc_Out_3_Vector4;
            Unity_Lerp_float4((_Multiply_9c8955b400df968fa3d38a4502565377_Out_2_Float.xxxx), _Multiply_f2ca1f6ea7179e8aa89b1e0b99e5aa31_Out_2_Vector4, (_Property_545e979a9125fa83bf79f3f29f090265_Out_0_Float.xxxx), _Lerp_9a69789c96a6db8c847dc8ffb41468bc_Out_3_Vector4);
            float4 _Absolute_3b487cec1b9a518ca94a075f77aaefe7_Out_1_Vector4;
            Unity_Absolute_float4(_Lerp_9a69789c96a6db8c847dc8ffb41468bc_Out_3_Vector4, _Absolute_3b487cec1b9a518ca94a075f77aaefe7_Out_1_Vector4);
            float _Property_0fd4bfc5c1229a8ca2b6b9c4986a9e64_Out_0_Float = _Emissionpower;
            float4 _Power_b9467dec8254ce8a963315301f8da311_Out_2_Vector4;
            Unity_Power_float4(_Absolute_3b487cec1b9a518ca94a075f77aaefe7_Out_1_Vector4, (_Property_0fd4bfc5c1229a8ca2b6b9c4986a9e64_Out_0_Float.xxxx), _Power_b9467dec8254ce8a963315301f8da311_Out_2_Vector4);
            float _Property_21bbaf00d2ccc38698a609ad485f67fa_Out_0_Float = _Emission;
            float4 _Multiply_b54d9efacf2c88829e5dfc4a0ffe1837_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Power_b9467dec8254ce8a963315301f8da311_Out_2_Vector4, (_Property_21bbaf00d2ccc38698a609ad485f67fa_Out_0_Float.xxxx), _Multiply_b54d9efacf2c88829e5dfc4a0ffe1837_Out_2_Vector4);
            float4 _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
            float4 _Multiply_df32f698e120fb8f822b54c5fab3878f_Out_2_Vector4;
            Unity_Multiply_float4_float4(IN.VertexColor, _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4, _Multiply_df32f698e120fb8f822b54c5fab3878f_Out_2_Vector4);
            float4 _Multiply_d562aca368957f83b86107a4542c7189_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_b54d9efacf2c88829e5dfc4a0ffe1837_Out_2_Vector4, _Multiply_df32f698e120fb8f822b54c5fab3878f_Out_2_Vector4, _Multiply_d562aca368957f83b86107a4542c7189_Out_2_Vector4);
            float _Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean = _Softedges;
            float _Split_08b2a371dca50b88abcbf5c2e7362375_R_1_Float = IN.VertexColor[0];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_G_2_Float = IN.VertexColor[1];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_B_3_Float = IN.VertexColor[2];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float = IN.VertexColor[3];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_R_1_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[0];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_G_2_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[1];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_B_3_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[2];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[3];
            float _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float;
            Unity_Multiply_float_float(_Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float, _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float, _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float);
            UnityTexture2D _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D = UnityBuildTexture2DStruct(_OpacityTex);
            float4 _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4 = _OpacityTexspeedXY;
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[0];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[1];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_B_3_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[2];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_A_4_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[3];
            float2 _Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2 = float2(_Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float, _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float);
            float2 _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2);
            float2 _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2, _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2);
            float4 _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.tex, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.samplerstate, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2) );
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.r;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_G_5_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.g;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_B_6_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.b;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_A_7_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.a;
            float _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float;
            Unity_Absolute_float(_SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float, _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float);
            float _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float = _Maskpower;
            float _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float;
            Unity_Power_float(_Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float, _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float, _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float);
            float _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float = _Maskmultiplayer;
            float _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float;
            Unity_Multiply_float_float(_Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float, _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float, _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float);
            float _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float;
            Unity_Clamp_float(_Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float, float(0), float(1), _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float);
            UnityTexture2D _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D = UnityBuildTexture2DStruct(_Mask);
            float4 _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.tex, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.samplerstate, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_R_4_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.r;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_G_5_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.g;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_B_6_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.b;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.a;
            float _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float;
            Unity_Multiply_float_float(_Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float, _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float);
            float _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float);
            float _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float;
            Unity_DotProduct_float3(IN.ObjectSpaceNormal, IN.ObjectSpaceViewDirection, _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float);
            float _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float;
            Unity_Absolute_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float);
            float _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float;
            Unity_Power_float(_Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float, float(3), _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float);
            float _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float = _Sideopacitymult;
            float _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float;
            Unity_Multiply_float_float(_Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float, _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float, _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float);
            float _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float;
            Unity_Clamp_float(_Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float, float(0), float(1), _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float);
            float _Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean = _Upopacity;
            float4 _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4 = IN.uv0;
            float _Split_50de07629cf17787826e85912d0e76d2_R_1_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[0];
            float _Split_50de07629cf17787826e85912d0e76d2_G_2_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[1];
            float _Split_50de07629cf17787826e85912d0e76d2_B_3_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[2];
            float _Split_50de07629cf17787826e85912d0e76d2_A_4_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[3];
            float _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float;
            Unity_Absolute_float(_Split_50de07629cf17787826e85912d0e76d2_G_2_Float, _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float);
            float _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float;
            Unity_Power_float(_Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float, float(4), _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float);
            float _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float;
            Unity_Multiply_float_float(_Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float, 3, _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float);
            float _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float;
            Unity_Clamp_float(_Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float, float(0), float(1), _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float);
            float _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float;
            Unity_Branch_float(_Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean, _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float, float(1), _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float);
            float _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float;
            Unity_Remap_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, float2 (0, -1), float2 (0, 1), _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float);
            float _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float;
            Unity_Multiply_float_float(_Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float, _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float);
            float _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float;
            Unity_Sign_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float);
            float _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float;
            Unity_Remap_float(_Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float, float2 (-1, 1), float2 (1, 0), _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float);
            float _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float;
            Unity_Lerp_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float, _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float, _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float);
            float _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float;
            Unity_Clamp_float(_Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float, float(0), float(1), _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float);
            float _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float);
            float _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            Unity_Branch_float(_Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float);
            surface.BaseColor = (_Multiply_d562aca368957f83b86107a4542c7189_Out_2_Vector4.xyz);
            surface.BaseColor =0.25;
            surface.Alpha = _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.ObjectSpaceNormal = normalize(mul(output.WorldSpaceNormal, (float3x3) UNITY_MATRIX_M));           // transposed multiplication by inverse matrix to handle normal scale
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.ObjectSpaceViewDirection = TransformWorldToObjectDir(output.WorldSpaceViewDirection);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "MotionVectors"
            Tags
            {
                "LightMode" = "MotionVectors"
            }
        
        // Render State
        Cull Off
        ZTest LEqual
        ZWrite On
        ColorMask RG
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.5
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_MOTION_VECTORS
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 WorldSpaceNormal;
             float3 ObjectSpaceViewDirection;
             float3 WorldSpaceViewDirection;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float3 positionWS : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _Numberofwaves;
        float4 _WavesspeedsizeXYTwistspeedsizeZW;
        float _VertexScale;
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
        float4 _Noise_TexelSize;
        float4 _Noise_ST;
        float4 _SpeedMainTexUVNoiseZW;
        float _Noisescale;
        float _Noisepower;
        float _Noiselerp;
        float4 _Color;
        float _Emissionpower;
        float _Emission;
        float4 _OpacityTex_TexelSize;
        float4 _OpacityTex_ST;
        float4 _Mask_TexelSize;
        float4 _Mask_ST;
        float _Maskpower;
        float _Maskmultiplayer;
        float _Softedges;
        float _Usedepth;
        float _Depthpower;
        float4 _OpacityTexspeedXY;
        float _Sideopacitymult;
        float _Upopacity;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_Noise);
        SAMPLER(sampler_Noise);
        TEXTURE2D(_OpacityTex);
        SAMPLER(sampler_OpacityTex);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Sign_float(float In, out float Out)
        {
            Out = sign(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean = _Softedges;
            float _Split_08b2a371dca50b88abcbf5c2e7362375_R_1_Float = IN.VertexColor[0];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_G_2_Float = IN.VertexColor[1];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_B_3_Float = IN.VertexColor[2];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float = IN.VertexColor[3];
            float4 _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
            float _Split_8576d58efb23248a96d3504ddf7eeb82_R_1_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[0];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_G_2_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[1];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_B_3_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[2];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[3];
            float _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float;
            Unity_Multiply_float_float(_Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float, _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float, _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float);
            UnityTexture2D _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D = UnityBuildTexture2DStruct(_OpacityTex);
            float4 _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4 = _OpacityTexspeedXY;
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[0];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[1];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_B_3_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[2];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_A_4_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[3];
            float2 _Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2 = float2(_Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float, _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float);
            float2 _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2);
            float2 _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2, _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2);
            float4 _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.tex, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.samplerstate, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2) );
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.r;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_G_5_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.g;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_B_6_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.b;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_A_7_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.a;
            float _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float;
            Unity_Absolute_float(_SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float, _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float);
            float _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float = _Maskpower;
            float _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float;
            Unity_Power_float(_Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float, _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float, _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float);
            float _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float = _Maskmultiplayer;
            float _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float;
            Unity_Multiply_float_float(_Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float, _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float, _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float);
            float _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float;
            Unity_Clamp_float(_Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float, float(0), float(1), _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float);
            UnityTexture2D _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D = UnityBuildTexture2DStruct(_Mask);
            float4 _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.tex, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.samplerstate, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_R_4_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.r;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_G_5_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.g;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_B_6_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.b;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.a;
            float _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float;
            Unity_Multiply_float_float(_Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float, _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float);
            float _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float);
            float _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float;
            Unity_DotProduct_float3(IN.ObjectSpaceNormal, IN.ObjectSpaceViewDirection, _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float);
            float _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float;
            Unity_Absolute_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float);
            float _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float;
            Unity_Power_float(_Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float, float(3), _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float);
            float _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float = _Sideopacitymult;
            float _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float;
            Unity_Multiply_float_float(_Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float, _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float, _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float);
            float _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float;
            Unity_Clamp_float(_Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float, float(0), float(1), _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float);
            float _Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean = _Upopacity;
            float4 _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4 = IN.uv0;
            float _Split_50de07629cf17787826e85912d0e76d2_R_1_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[0];
            float _Split_50de07629cf17787826e85912d0e76d2_G_2_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[1];
            float _Split_50de07629cf17787826e85912d0e76d2_B_3_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[2];
            float _Split_50de07629cf17787826e85912d0e76d2_A_4_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[3];
            float _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float;
            Unity_Absolute_float(_Split_50de07629cf17787826e85912d0e76d2_G_2_Float, _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float);
            float _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float;
            Unity_Power_float(_Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float, float(4), _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float);
            float _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float;
            Unity_Multiply_float_float(_Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float, 3, _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float);
            float _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float;
            Unity_Clamp_float(_Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float, float(0), float(1), _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float);
            float _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float;
            Unity_Branch_float(_Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean, _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float, float(1), _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float);
            float _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float;
            Unity_Remap_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, float2 (0, -1), float2 (0, 1), _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float);
            float _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float;
            Unity_Multiply_float_float(_Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float, _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float);
            float _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float;
            Unity_Sign_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float);
            float _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float;
            Unity_Remap_float(_Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float, float2 (-1, 1), float2 (1, 0), _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float);
            float _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float;
            Unity_Lerp_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float, _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float, _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float);
            float _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float;
            Unity_Clamp_float(_Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float, float(0), float(1), _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float);
            float _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float);
            float _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            Unity_Branch_float(_Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float);
            surface.Alpha = _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.ObjectSpaceNormal = normalize(mul(output.WorldSpaceNormal, (float3x3) UNITY_MATRIX_M));           // transposed multiplication by inverse matrix to handle normal scale
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.ObjectSpaceViewDirection = TransformWorldToObjectDir(output.WorldSpaceViewDirection);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/MotionVectorPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthNormalsOnly"
            Tags
            {
                "LightMode" = "DepthNormalsOnly"
            }
        
        // Render State
        Cull Off
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
        #define _SURFACE_TYPE_TRANSPARENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 WorldSpaceNormal;
             float3 ObjectSpaceViewDirection;
             float3 WorldSpaceViewDirection;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float3 positionWS : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _Numberofwaves;
        float4 _WavesspeedsizeXYTwistspeedsizeZW;
        float _VertexScale;
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
        float4 _Noise_TexelSize;
        float4 _Noise_ST;
        float4 _SpeedMainTexUVNoiseZW;
        float _Noisescale;
        float _Noisepower;
        float _Noiselerp;
        float4 _Color;
        float _Emissionpower;
        float _Emission;
        float4 _OpacityTex_TexelSize;
        float4 _OpacityTex_ST;
        float4 _Mask_TexelSize;
        float4 _Mask_ST;
        float _Maskpower;
        float _Maskmultiplayer;
        float _Softedges;
        float _Usedepth;
        float _Depthpower;
        float4 _OpacityTexspeedXY;
        float _Sideopacitymult;
        float _Upopacity;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_Noise);
        SAMPLER(sampler_Noise);
        TEXTURE2D(_OpacityTex);
        SAMPLER(sampler_OpacityTex);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Sign_float(float In, out float Out)
        {
            Out = sign(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean = _Softedges;
            float _Split_08b2a371dca50b88abcbf5c2e7362375_R_1_Float = IN.VertexColor[0];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_G_2_Float = IN.VertexColor[1];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_B_3_Float = IN.VertexColor[2];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float = IN.VertexColor[3];
            float4 _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
            float _Split_8576d58efb23248a96d3504ddf7eeb82_R_1_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[0];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_G_2_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[1];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_B_3_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[2];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[3];
            float _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float;
            Unity_Multiply_float_float(_Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float, _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float, _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float);
            UnityTexture2D _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D = UnityBuildTexture2DStruct(_OpacityTex);
            float4 _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4 = _OpacityTexspeedXY;
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[0];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[1];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_B_3_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[2];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_A_4_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[3];
            float2 _Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2 = float2(_Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float, _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float);
            float2 _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2);
            float2 _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2, _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2);
            float4 _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.tex, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.samplerstate, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2) );
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.r;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_G_5_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.g;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_B_6_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.b;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_A_7_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.a;
            float _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float;
            Unity_Absolute_float(_SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float, _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float);
            float _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float = _Maskpower;
            float _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float;
            Unity_Power_float(_Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float, _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float, _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float);
            float _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float = _Maskmultiplayer;
            float _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float;
            Unity_Multiply_float_float(_Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float, _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float, _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float);
            float _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float;
            Unity_Clamp_float(_Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float, float(0), float(1), _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float);
            UnityTexture2D _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D = UnityBuildTexture2DStruct(_Mask);
            float4 _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.tex, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.samplerstate, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_R_4_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.r;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_G_5_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.g;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_B_6_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.b;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.a;
            float _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float;
            Unity_Multiply_float_float(_Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float, _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float);
            float _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float);
            float _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float;
            Unity_DotProduct_float3(IN.ObjectSpaceNormal, IN.ObjectSpaceViewDirection, _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float);
            float _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float;
            Unity_Absolute_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float);
            float _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float;
            Unity_Power_float(_Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float, float(3), _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float);
            float _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float = _Sideopacitymult;
            float _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float;
            Unity_Multiply_float_float(_Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float, _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float, _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float);
            float _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float;
            Unity_Clamp_float(_Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float, float(0), float(1), _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float);
            float _Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean = _Upopacity;
            float4 _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4 = IN.uv0;
            float _Split_50de07629cf17787826e85912d0e76d2_R_1_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[0];
            float _Split_50de07629cf17787826e85912d0e76d2_G_2_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[1];
            float _Split_50de07629cf17787826e85912d0e76d2_B_3_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[2];
            float _Split_50de07629cf17787826e85912d0e76d2_A_4_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[3];
            float _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float;
            Unity_Absolute_float(_Split_50de07629cf17787826e85912d0e76d2_G_2_Float, _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float);
            float _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float;
            Unity_Power_float(_Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float, float(4), _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float);
            float _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float;
            Unity_Multiply_float_float(_Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float, 3, _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float);
            float _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float;
            Unity_Clamp_float(_Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float, float(0), float(1), _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float);
            float _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float;
            Unity_Branch_float(_Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean, _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float, float(1), _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float);
            float _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float;
            Unity_Remap_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, float2 (0, -1), float2 (0, 1), _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float);
            float _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float;
            Unity_Multiply_float_float(_Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float, _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float);
            float _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float;
            Unity_Sign_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float);
            float _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float;
            Unity_Remap_float(_Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float, float2 (-1, 1), float2 (1, 0), _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float);
            float _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float;
            Unity_Lerp_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float, _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float, _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float);
            float _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float;
            Unity_Clamp_float(_Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float, float(0), float(1), _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float);
            float _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float);
            float _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            Unity_Branch_float(_Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float);
            surface.Alpha = _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.ObjectSpaceNormal = normalize(mul(output.WorldSpaceNormal, (float3x3) UNITY_MATRIX_M));           // transposed multiplication by inverse matrix to handle normal scale
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.ObjectSpaceViewDirection = TransformWorldToObjectDir(output.WorldSpaceViewDirection);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_GBUFFER
        #define _SURFACE_TYPE_TRANSPARENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 color;
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 WorldSpaceNormal;
             float3 ObjectSpaceViewDirection;
             float3 WorldSpaceViewDirection;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP0;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion : INTERP1;
            #endif
             float4 texCoord0 : INTERP2;
             float4 color : INTERP3;
             float3 positionWS : INTERP4;
             float3 normalWS : INTERP5;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _Numberofwaves;
        float4 _WavesspeedsizeXYTwistspeedsizeZW;
        float _VertexScale;
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
        float4 _Noise_TexelSize;
        float4 _Noise_ST;
        float4 _SpeedMainTexUVNoiseZW;
        float _Noisescale;
        float _Noisepower;
        float _Noiselerp;
        float4 _Color;
        float _Emissionpower;
        float _Emission;
        float4 _OpacityTex_TexelSize;
        float4 _OpacityTex_ST;
        float4 _Mask_TexelSize;
        float4 _Mask_ST;
        float _Maskpower;
        float _Maskmultiplayer;
        float _Softedges;
        float _Usedepth;
        float _Depthpower;
        float4 _OpacityTexspeedXY;
        float _Sideopacitymult;
        float _Upopacity;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_Noise);
        SAMPLER(sampler_Noise);
        TEXTURE2D(_OpacityTex);
        SAMPLER(sampler_OpacityTex);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        float2 Unity_Voronoi_RandomVector_LegacySine_float (float2 UV, float offset)
        {
            Hash_LegacySine_2_2_float(UV, UV);
            return float2(sin(UV.y * offset), cos(UV.x * offset)) * 0.5 + 0.5;
        }
        
        void Unity_Voronoi_LegacySine_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    float2 lattice = float2(x, y);
                    float2 offset = Unity_Voronoi_RandomVector_LegacySine_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);
                    if (d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Sign_float(float In, out float Out)
        {
            Out = sign(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_44f0c1ab07e6a485baa8263505d3561c_Out_0_Float = _Noisepower;
            float _Property_e671dda5b7d5728d8daa9041adfc7ba8_Out_0_Float = _Noisescale;
            float _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Out_3_Float;
            float _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Cells_4_Float;
            Unity_Voronoi_LegacySine_float(IN.uv0.xy, float(100), _Property_e671dda5b7d5728d8daa9041adfc7ba8_Out_0_Float, _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Out_3_Float, _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Cells_4_Float);
            float _Multiply_9c8955b400df968fa3d38a4502565377_Out_2_Float;
            Unity_Multiply_float_float(_Property_44f0c1ab07e6a485baa8263505d3561c_Out_0_Float, _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Out_3_Float, _Multiply_9c8955b400df968fa3d38a4502565377_Out_2_Float);
            UnityTexture2D _Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float4 _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4 = _SpeedMainTexUVNoiseZW;
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_R_1_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[0];
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_G_2_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[1];
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_B_3_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[2];
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_A_4_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[3];
            float2 _Vector2_00da01662a67968f944c0070caa8dc55_Out_0_Vector2 = float2(_Split_e997a8b001c94f8c89b40fb8ed45334a_R_1_Float, _Split_e997a8b001c94f8c89b40fb8ed45334a_G_2_Float);
            float2 _Multiply_af92727bf284c08c9960c739abde2c0f_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Vector2_00da01662a67968f944c0070caa8dc55_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_af92727bf284c08c9960c739abde2c0f_Out_2_Vector2);
            float2 _TilingAndOffset_785428c855ac1383a0f7650b556ad228_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_af92727bf284c08c9960c739abde2c0f_Out_2_Vector2, _TilingAndOffset_785428c855ac1383a0f7650b556ad228_Out_3_Vector2);
            float4 _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D.tex, _Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D.samplerstate, _Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_785428c855ac1383a0f7650b556ad228_Out_3_Vector2) );
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_R_4_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.r;
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_G_5_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.g;
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_B_6_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.b;
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_A_7_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.a;
            UnityTexture2D _Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D = UnityBuildTexture2DStruct(_Noise);
            float2 _Vector2_912722ece3bcfb8c9b66aa407cd25dc6_Out_0_Vector2 = float2(_Split_e997a8b001c94f8c89b40fb8ed45334a_B_3_Float, _Split_e997a8b001c94f8c89b40fb8ed45334a_A_4_Float);
            float2 _Multiply_a7450d1dc073518d8474ac32f7624bf9_Out_2_Vector2;
            Unity_Multiply_float2_float2((IN.TimeParameters.x.xx), _Vector2_912722ece3bcfb8c9b66aa407cd25dc6_Out_0_Vector2, _Multiply_a7450d1dc073518d8474ac32f7624bf9_Out_2_Vector2);
            float2 _TilingAndOffset_1b734d5536df74898bf84054510831de_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_a7450d1dc073518d8474ac32f7624bf9_Out_2_Vector2, _TilingAndOffset_1b734d5536df74898bf84054510831de_Out_3_Vector2);
            float4 _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D.tex, _Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D.samplerstate, _Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_1b734d5536df74898bf84054510831de_Out_3_Vector2) );
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_R_4_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.r;
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_G_5_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.g;
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_B_6_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.b;
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_A_7_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.a;
            float4 _Multiply_f2ca1f6ea7179e8aa89b1e0b99e5aa31_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4, _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4, _Multiply_f2ca1f6ea7179e8aa89b1e0b99e5aa31_Out_2_Vector4);
            float _Property_545e979a9125fa83bf79f3f29f090265_Out_0_Float = _Noiselerp;
            float4 _Lerp_9a69789c96a6db8c847dc8ffb41468bc_Out_3_Vector4;
            Unity_Lerp_float4((_Multiply_9c8955b400df968fa3d38a4502565377_Out_2_Float.xxxx), _Multiply_f2ca1f6ea7179e8aa89b1e0b99e5aa31_Out_2_Vector4, (_Property_545e979a9125fa83bf79f3f29f090265_Out_0_Float.xxxx), _Lerp_9a69789c96a6db8c847dc8ffb41468bc_Out_3_Vector4);
            float4 _Absolute_3b487cec1b9a518ca94a075f77aaefe7_Out_1_Vector4;
            Unity_Absolute_float4(_Lerp_9a69789c96a6db8c847dc8ffb41468bc_Out_3_Vector4, _Absolute_3b487cec1b9a518ca94a075f77aaefe7_Out_1_Vector4);
            float _Property_0fd4bfc5c1229a8ca2b6b9c4986a9e64_Out_0_Float = _Emissionpower;
            float4 _Power_b9467dec8254ce8a963315301f8da311_Out_2_Vector4;
            Unity_Power_float4(_Absolute_3b487cec1b9a518ca94a075f77aaefe7_Out_1_Vector4, (_Property_0fd4bfc5c1229a8ca2b6b9c4986a9e64_Out_0_Float.xxxx), _Power_b9467dec8254ce8a963315301f8da311_Out_2_Vector4);
            float _Property_21bbaf00d2ccc38698a609ad485f67fa_Out_0_Float = _Emission;
            float4 _Multiply_b54d9efacf2c88829e5dfc4a0ffe1837_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Power_b9467dec8254ce8a963315301f8da311_Out_2_Vector4, (_Property_21bbaf00d2ccc38698a609ad485f67fa_Out_0_Float.xxxx), _Multiply_b54d9efacf2c88829e5dfc4a0ffe1837_Out_2_Vector4);
            float4 _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
            float4 _Multiply_df32f698e120fb8f822b54c5fab3878f_Out_2_Vector4;
            Unity_Multiply_float4_float4(IN.VertexColor, _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4, _Multiply_df32f698e120fb8f822b54c5fab3878f_Out_2_Vector4);
            float4 _Multiply_d562aca368957f83b86107a4542c7189_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_b54d9efacf2c88829e5dfc4a0ffe1837_Out_2_Vector4, _Multiply_df32f698e120fb8f822b54c5fab3878f_Out_2_Vector4, _Multiply_d562aca368957f83b86107a4542c7189_Out_2_Vector4);
            float _Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean = _Softedges;
            float _Split_08b2a371dca50b88abcbf5c2e7362375_R_1_Float = IN.VertexColor[0];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_G_2_Float = IN.VertexColor[1];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_B_3_Float = IN.VertexColor[2];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float = IN.VertexColor[3];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_R_1_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[0];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_G_2_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[1];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_B_3_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[2];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[3];
            float _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float;
            Unity_Multiply_float_float(_Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float, _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float, _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float);
            UnityTexture2D _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D = UnityBuildTexture2DStruct(_OpacityTex);
            float4 _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4 = _OpacityTexspeedXY;
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[0];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[1];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_B_3_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[2];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_A_4_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[3];
            float2 _Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2 = float2(_Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float, _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float);
            float2 _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2);
            float2 _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2, _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2);
            float4 _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.tex, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.samplerstate, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2) );
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.r;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_G_5_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.g;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_B_6_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.b;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_A_7_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.a;
            float _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float;
            Unity_Absolute_float(_SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float, _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float);
            float _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float = _Maskpower;
            float _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float;
            Unity_Power_float(_Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float, _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float, _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float);
            float _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float = _Maskmultiplayer;
            float _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float;
            Unity_Multiply_float_float(_Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float, _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float, _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float);
            float _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float;
            Unity_Clamp_float(_Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float, float(0), float(1), _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float);
            UnityTexture2D _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D = UnityBuildTexture2DStruct(_Mask);
            float4 _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.tex, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.samplerstate, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_R_4_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.r;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_G_5_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.g;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_B_6_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.b;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.a;
            float _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float;
            Unity_Multiply_float_float(_Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float, _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float);
            float _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float);
            float _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float;
            Unity_DotProduct_float3(IN.ObjectSpaceNormal, IN.ObjectSpaceViewDirection, _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float);
            float _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float;
            Unity_Absolute_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float);
            float _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float;
            Unity_Power_float(_Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float, float(3), _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float);
            float _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float = _Sideopacitymult;
            float _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float;
            Unity_Multiply_float_float(_Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float, _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float, _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float);
            float _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float;
            Unity_Clamp_float(_Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float, float(0), float(1), _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float);
            float _Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean = _Upopacity;
            float4 _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4 = IN.uv0;
            float _Split_50de07629cf17787826e85912d0e76d2_R_1_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[0];
            float _Split_50de07629cf17787826e85912d0e76d2_G_2_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[1];
            float _Split_50de07629cf17787826e85912d0e76d2_B_3_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[2];
            float _Split_50de07629cf17787826e85912d0e76d2_A_4_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[3];
            float _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float;
            Unity_Absolute_float(_Split_50de07629cf17787826e85912d0e76d2_G_2_Float, _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float);
            float _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float;
            Unity_Power_float(_Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float, float(4), _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float);
            float _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float;
            Unity_Multiply_float_float(_Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float, 3, _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float);
            float _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float;
            Unity_Clamp_float(_Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float, float(0), float(1), _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float);
            float _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float;
            Unity_Branch_float(_Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean, _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float, float(1), _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float);
            float _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float;
            Unity_Remap_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, float2 (0, -1), float2 (0, 1), _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float);
            float _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float;
            Unity_Multiply_float_float(_Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float, _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float);
            float _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float;
            Unity_Sign_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float);
            float _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float;
            Unity_Remap_float(_Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float, float2 (-1, 1), float2 (1, 0), _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float);
            float _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float;
            Unity_Lerp_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float, _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float, _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float);
            float _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float;
            Unity_Clamp_float(_Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float, float(0), float(1), _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float);
            float _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float);
            float _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            Unity_Branch_float(_Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float);
            surface.BaseColor = (_Multiply_d562aca368957f83b86107a4542c7189_Out_2_Vector4.xyz);
            surface.Alpha = _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.ObjectSpaceNormal = normalize(mul(output.WorldSpaceNormal, (float3x3) UNITY_MATRIX_M));           // transposed multiplication by inverse matrix to handle normal scale
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.ObjectSpaceViewDirection = TransformWorldToObjectDir(output.WorldSpaceViewDirection);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitGBufferPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 WorldSpaceNormal;
             float3 ObjectSpaceViewDirection;
             float3 WorldSpaceViewDirection;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float3 positionWS : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _Numberofwaves;
        float4 _WavesspeedsizeXYTwistspeedsizeZW;
        float _VertexScale;
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
        float4 _Noise_TexelSize;
        float4 _Noise_ST;
        float4 _SpeedMainTexUVNoiseZW;
        float _Noisescale;
        float _Noisepower;
        float _Noiselerp;
        float4 _Color;
        float _Emissionpower;
        float _Emission;
        float4 _OpacityTex_TexelSize;
        float4 _OpacityTex_ST;
        float4 _Mask_TexelSize;
        float4 _Mask_ST;
        float _Maskpower;
        float _Maskmultiplayer;
        float _Softedges;
        float _Usedepth;
        float _Depthpower;
        float4 _OpacityTexspeedXY;
        float _Sideopacitymult;
        float _Upopacity;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_Noise);
        SAMPLER(sampler_Noise);
        TEXTURE2D(_OpacityTex);
        SAMPLER(sampler_OpacityTex);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Sign_float(float In, out float Out)
        {
            Out = sign(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean = _Softedges;
            float _Split_08b2a371dca50b88abcbf5c2e7362375_R_1_Float = IN.VertexColor[0];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_G_2_Float = IN.VertexColor[1];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_B_3_Float = IN.VertexColor[2];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float = IN.VertexColor[3];
            float4 _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
            float _Split_8576d58efb23248a96d3504ddf7eeb82_R_1_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[0];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_G_2_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[1];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_B_3_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[2];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[3];
            float _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float;
            Unity_Multiply_float_float(_Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float, _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float, _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float);
            UnityTexture2D _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D = UnityBuildTexture2DStruct(_OpacityTex);
            float4 _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4 = _OpacityTexspeedXY;
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[0];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[1];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_B_3_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[2];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_A_4_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[3];
            float2 _Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2 = float2(_Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float, _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float);
            float2 _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2);
            float2 _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2, _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2);
            float4 _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.tex, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.samplerstate, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2) );
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.r;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_G_5_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.g;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_B_6_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.b;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_A_7_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.a;
            float _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float;
            Unity_Absolute_float(_SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float, _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float);
            float _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float = _Maskpower;
            float _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float;
            Unity_Power_float(_Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float, _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float, _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float);
            float _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float = _Maskmultiplayer;
            float _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float;
            Unity_Multiply_float_float(_Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float, _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float, _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float);
            float _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float;
            Unity_Clamp_float(_Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float, float(0), float(1), _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float);
            UnityTexture2D _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D = UnityBuildTexture2DStruct(_Mask);
            float4 _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.tex, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.samplerstate, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_R_4_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.r;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_G_5_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.g;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_B_6_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.b;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.a;
            float _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float;
            Unity_Multiply_float_float(_Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float, _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float);
            float _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float);
            float _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float;
            Unity_DotProduct_float3(IN.ObjectSpaceNormal, IN.ObjectSpaceViewDirection, _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float);
            float _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float;
            Unity_Absolute_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float);
            float _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float;
            Unity_Power_float(_Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float, float(3), _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float);
            float _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float = _Sideopacitymult;
            float _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float;
            Unity_Multiply_float_float(_Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float, _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float, _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float);
            float _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float;
            Unity_Clamp_float(_Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float, float(0), float(1), _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float);
            float _Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean = _Upopacity;
            float4 _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4 = IN.uv0;
            float _Split_50de07629cf17787826e85912d0e76d2_R_1_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[0];
            float _Split_50de07629cf17787826e85912d0e76d2_G_2_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[1];
            float _Split_50de07629cf17787826e85912d0e76d2_B_3_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[2];
            float _Split_50de07629cf17787826e85912d0e76d2_A_4_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[3];
            float _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float;
            Unity_Absolute_float(_Split_50de07629cf17787826e85912d0e76d2_G_2_Float, _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float);
            float _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float;
            Unity_Power_float(_Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float, float(4), _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float);
            float _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float;
            Unity_Multiply_float_float(_Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float, 3, _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float);
            float _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float;
            Unity_Clamp_float(_Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float, float(0), float(1), _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float);
            float _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float;
            Unity_Branch_float(_Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean, _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float, float(1), _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float);
            float _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float;
            Unity_Remap_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, float2 (0, -1), float2 (0, 1), _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float);
            float _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float;
            Unity_Multiply_float_float(_Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float, _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float);
            float _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float;
            Unity_Sign_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float);
            float _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float;
            Unity_Remap_float(_Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float, float2 (-1, 1), float2 (1, 0), _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float);
            float _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float;
            Unity_Lerp_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float, _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float, _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float);
            float _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float;
            Unity_Clamp_float(_Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float, float(0), float(1), _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float);
            float _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float);
            float _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            Unity_Branch_float(_Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float);
            surface.Alpha = _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.ObjectSpaceNormal = normalize(mul(output.WorldSpaceNormal, (float3x3) UNITY_MATRIX_M));           // transposed multiplication by inverse matrix to handle normal scale
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.ObjectSpaceViewDirection = TransformWorldToObjectDir(output.WorldSpaceViewDirection);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 WorldSpaceNormal;
             float3 ObjectSpaceViewDirection;
             float3 WorldSpaceViewDirection;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float3 positionWS : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _Numberofwaves;
        float4 _WavesspeedsizeXYTwistspeedsizeZW;
        float _VertexScale;
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
        float4 _Noise_TexelSize;
        float4 _Noise_ST;
        float4 _SpeedMainTexUVNoiseZW;
        float _Noisescale;
        float _Noisepower;
        float _Noiselerp;
        float4 _Color;
        float _Emissionpower;
        float _Emission;
        float4 _OpacityTex_TexelSize;
        float4 _OpacityTex_ST;
        float4 _Mask_TexelSize;
        float4 _Mask_ST;
        float _Maskpower;
        float _Maskmultiplayer;
        float _Softedges;
        float _Usedepth;
        float _Depthpower;
        float4 _OpacityTexspeedXY;
        float _Sideopacitymult;
        float _Upopacity;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_Noise);
        SAMPLER(sampler_Noise);
        TEXTURE2D(_OpacityTex);
        SAMPLER(sampler_OpacityTex);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        float2 Unity_Voronoi_RandomVector_LegacySine_float (float2 UV, float offset)
        {
            Hash_LegacySine_2_2_float(UV, UV);
            return float2(sin(UV.y * offset), cos(UV.x * offset)) * 0.5 + 0.5;
        }
        
        void Unity_Voronoi_LegacySine_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    float2 lattice = float2(x, y);
                    float2 offset = Unity_Voronoi_RandomVector_LegacySine_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);
                    if (d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Sign_float(float In, out float Out)
        {
            Out = sign(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_44f0c1ab07e6a485baa8263505d3561c_Out_0_Float = _Noisepower;
            float _Property_e671dda5b7d5728d8daa9041adfc7ba8_Out_0_Float = _Noisescale;
            float _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Out_3_Float;
            float _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Cells_4_Float;
            Unity_Voronoi_LegacySine_float(IN.uv0.xy, float(100), _Property_e671dda5b7d5728d8daa9041adfc7ba8_Out_0_Float, _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Out_3_Float, _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Cells_4_Float);
            float _Multiply_9c8955b400df968fa3d38a4502565377_Out_2_Float;
            Unity_Multiply_float_float(_Property_44f0c1ab07e6a485baa8263505d3561c_Out_0_Float, _Voronoi_d68c221c0b004de7b8a0aad59fe2126c_Out_3_Float, _Multiply_9c8955b400df968fa3d38a4502565377_Out_2_Float);
            UnityTexture2D _Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D = UnityBuildTexture2DStruct(_MainTex);
            float4 _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4 = _SpeedMainTexUVNoiseZW;
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_R_1_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[0];
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_G_2_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[1];
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_B_3_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[2];
            float _Split_e997a8b001c94f8c89b40fb8ed45334a_A_4_Float = _Property_85b8c58e85972789802dfaea97ad05e0_Out_0_Vector4[3];
            float2 _Vector2_00da01662a67968f944c0070caa8dc55_Out_0_Vector2 = float2(_Split_e997a8b001c94f8c89b40fb8ed45334a_R_1_Float, _Split_e997a8b001c94f8c89b40fb8ed45334a_G_2_Float);
            float2 _Multiply_af92727bf284c08c9960c739abde2c0f_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Vector2_00da01662a67968f944c0070caa8dc55_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_af92727bf284c08c9960c739abde2c0f_Out_2_Vector2);
            float2 _TilingAndOffset_785428c855ac1383a0f7650b556ad228_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_af92727bf284c08c9960c739abde2c0f_Out_2_Vector2, _TilingAndOffset_785428c855ac1383a0f7650b556ad228_Out_3_Vector2);
            float4 _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D.tex, _Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D.samplerstate, _Property_5a5564758aac3e8ab0f2bbdc40bb431b_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_785428c855ac1383a0f7650b556ad228_Out_3_Vector2) );
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_R_4_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.r;
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_G_5_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.g;
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_B_6_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.b;
            float _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_A_7_Float = _SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4.a;
            UnityTexture2D _Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D = UnityBuildTexture2DStruct(_Noise);
            float2 _Vector2_912722ece3bcfb8c9b66aa407cd25dc6_Out_0_Vector2 = float2(_Split_e997a8b001c94f8c89b40fb8ed45334a_B_3_Float, _Split_e997a8b001c94f8c89b40fb8ed45334a_A_4_Float);
            float2 _Multiply_a7450d1dc073518d8474ac32f7624bf9_Out_2_Vector2;
            Unity_Multiply_float2_float2((IN.TimeParameters.x.xx), _Vector2_912722ece3bcfb8c9b66aa407cd25dc6_Out_0_Vector2, _Multiply_a7450d1dc073518d8474ac32f7624bf9_Out_2_Vector2);
            float2 _TilingAndOffset_1b734d5536df74898bf84054510831de_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_a7450d1dc073518d8474ac32f7624bf9_Out_2_Vector2, _TilingAndOffset_1b734d5536df74898bf84054510831de_Out_3_Vector2);
            float4 _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D.tex, _Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D.samplerstate, _Property_20aa464a8a0ea88cb5c381b8831730cb_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_1b734d5536df74898bf84054510831de_Out_3_Vector2) );
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_R_4_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.r;
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_G_5_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.g;
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_B_6_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.b;
            float _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_A_7_Float = _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4.a;
            float4 _Multiply_f2ca1f6ea7179e8aa89b1e0b99e5aa31_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_b82d73ca02e1248b9b2a9d7c78ef7e64_RGBA_0_Vector4, _SampleTexture2D_f717e5f65c8d328a89b5d6e349d5ddda_RGBA_0_Vector4, _Multiply_f2ca1f6ea7179e8aa89b1e0b99e5aa31_Out_2_Vector4);
            float _Property_545e979a9125fa83bf79f3f29f090265_Out_0_Float = _Noiselerp;
            float4 _Lerp_9a69789c96a6db8c847dc8ffb41468bc_Out_3_Vector4;
            Unity_Lerp_float4((_Multiply_9c8955b400df968fa3d38a4502565377_Out_2_Float.xxxx), _Multiply_f2ca1f6ea7179e8aa89b1e0b99e5aa31_Out_2_Vector4, (_Property_545e979a9125fa83bf79f3f29f090265_Out_0_Float.xxxx), _Lerp_9a69789c96a6db8c847dc8ffb41468bc_Out_3_Vector4);
            float4 _Absolute_3b487cec1b9a518ca94a075f77aaefe7_Out_1_Vector4;
            Unity_Absolute_float4(_Lerp_9a69789c96a6db8c847dc8ffb41468bc_Out_3_Vector4, _Absolute_3b487cec1b9a518ca94a075f77aaefe7_Out_1_Vector4);
            float _Property_0fd4bfc5c1229a8ca2b6b9c4986a9e64_Out_0_Float = _Emissionpower;
            float4 _Power_b9467dec8254ce8a963315301f8da311_Out_2_Vector4;
            Unity_Power_float4(_Absolute_3b487cec1b9a518ca94a075f77aaefe7_Out_1_Vector4, (_Property_0fd4bfc5c1229a8ca2b6b9c4986a9e64_Out_0_Float.xxxx), _Power_b9467dec8254ce8a963315301f8da311_Out_2_Vector4);
            float _Property_21bbaf00d2ccc38698a609ad485f67fa_Out_0_Float = _Emission;
            float4 _Multiply_b54d9efacf2c88829e5dfc4a0ffe1837_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Power_b9467dec8254ce8a963315301f8da311_Out_2_Vector4, (_Property_21bbaf00d2ccc38698a609ad485f67fa_Out_0_Float.xxxx), _Multiply_b54d9efacf2c88829e5dfc4a0ffe1837_Out_2_Vector4);
            float4 _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
            float4 _Multiply_df32f698e120fb8f822b54c5fab3878f_Out_2_Vector4;
            Unity_Multiply_float4_float4(IN.VertexColor, _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4, _Multiply_df32f698e120fb8f822b54c5fab3878f_Out_2_Vector4);
            float4 _Multiply_d562aca368957f83b86107a4542c7189_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_b54d9efacf2c88829e5dfc4a0ffe1837_Out_2_Vector4, _Multiply_df32f698e120fb8f822b54c5fab3878f_Out_2_Vector4, _Multiply_d562aca368957f83b86107a4542c7189_Out_2_Vector4);
            float _Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean = _Softedges;
            float _Split_08b2a371dca50b88abcbf5c2e7362375_R_1_Float = IN.VertexColor[0];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_G_2_Float = IN.VertexColor[1];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_B_3_Float = IN.VertexColor[2];
            float _Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float = IN.VertexColor[3];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_R_1_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[0];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_G_2_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[1];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_B_3_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[2];
            float _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float = _Property_fe00e731606ba48d9a9fa9df083222cd_Out_0_Vector4[3];
            float _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float;
            Unity_Multiply_float_float(_Split_08b2a371dca50b88abcbf5c2e7362375_A_4_Float, _Split_8576d58efb23248a96d3504ddf7eeb82_A_4_Float, _Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float);
            UnityTexture2D _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D = UnityBuildTexture2DStruct(_OpacityTex);
            float4 _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4 = _OpacityTexspeedXY;
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[0];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[1];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_B_3_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[2];
            float _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_A_4_Float = _Property_d59317d93b18cc8a9f4e5afd4b9f69da_Out_0_Vector4[3];
            float2 _Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2 = float2(_Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_R_1_Float, _Split_6a9bb8cfbbdd268fae6fc6b41c5985ee_G_2_Float);
            float2 _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2;
            Unity_Multiply_float2_float2(_Vector2_36c630459e1718848d7773b30dc8e2a9_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2);
            float2 _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Multiply_664b1813b6342b89a0b8ec323ed03934_Out_2_Vector2, _TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2);
            float4 _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.tex, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.samplerstate, _Property_ac08417ff7cdaf87bc8fe4eca659e66b_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_d30b64ff26e85382875cf3b3e2e660b9_Out_3_Vector2) );
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.r;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_G_5_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.g;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_B_6_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.b;
            float _SampleTexture2D_481de0df4615d582881fa7f2739b2514_A_7_Float = _SampleTexture2D_481de0df4615d582881fa7f2739b2514_RGBA_0_Vector4.a;
            float _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float;
            Unity_Absolute_float(_SampleTexture2D_481de0df4615d582881fa7f2739b2514_R_4_Float, _Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float);
            float _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float = _Maskpower;
            float _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float;
            Unity_Power_float(_Absolute_036eed3280c9dd899a03c65879195a4b_Out_1_Float, _Property_26d19d956c832a8e972fe9c6154d0444_Out_0_Float, _Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float);
            float _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float = _Maskmultiplayer;
            float _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float;
            Unity_Multiply_float_float(_Power_51b2111123487889b9a027cd8afb2bf4_Out_2_Float, _Property_76969ce9806e998796e7c6ef7fc76350_Out_0_Float, _Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float);
            float _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float;
            Unity_Clamp_float(_Multiply_bc59d6611c686784aa357ebb5fe536ac_Out_2_Float, float(0), float(1), _Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float);
            UnityTexture2D _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D = UnityBuildTexture2DStruct(_Mask);
            float4 _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.tex, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.samplerstate, _Property_14e3bc7b97b86381a0d0bfe4761f1a3a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_R_4_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.r;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_G_5_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.g;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_B_6_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.b;
            float _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float = _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_RGBA_0_Vector4.a;
            float _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float;
            Unity_Multiply_float_float(_Clamp_783d71e9a6e2fc898732d2eb24a37977_Out_3_Float, _SampleTexture2D_aea930039d9ffb8398eb97fe4cccc4c2_A_7_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float);
            float _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_55816b51bc8ef484b27119dd99cac061_Out_2_Float, _Multiply_d83d897619bb51809a54d18da2df697f_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float);
            float _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float;
            Unity_DotProduct_float3(IN.ObjectSpaceNormal, IN.ObjectSpaceViewDirection, _DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float);
            float _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float;
            Unity_Absolute_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float);
            float _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float;
            Unity_Power_float(_Absolute_f1f68933569db38b9936940120724ad6_Out_1_Float, float(3), _Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float);
            float _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float = _Sideopacitymult;
            float _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float;
            Unity_Multiply_float_float(_Power_23787fad9ef85787902d325b490e4bb3_Out_2_Float, _Property_3f22fc0e4124460aa0b87328a32fa804_Out_0_Float, _Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float);
            float _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float;
            Unity_Clamp_float(_Multiply_65757677f108668f8f4cc4df6b1156dd_Out_2_Float, float(0), float(1), _Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float);
            float _Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean = _Upopacity;
            float4 _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4 = IN.uv0;
            float _Split_50de07629cf17787826e85912d0e76d2_R_1_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[0];
            float _Split_50de07629cf17787826e85912d0e76d2_G_2_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[1];
            float _Split_50de07629cf17787826e85912d0e76d2_B_3_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[2];
            float _Split_50de07629cf17787826e85912d0e76d2_A_4_Float = _UV_0396185cc896428ea89f1c61426b2626_Out_0_Vector4[3];
            float _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float;
            Unity_Absolute_float(_Split_50de07629cf17787826e85912d0e76d2_G_2_Float, _Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float);
            float _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float;
            Unity_Power_float(_Absolute_6bc25874a2e09f85919a14de0d035b22_Out_1_Float, float(4), _Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float);
            float _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float;
            Unity_Multiply_float_float(_Power_528099fd22a4068b80c39ebc47fed37f_Out_2_Float, 3, _Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float);
            float _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float;
            Unity_Clamp_float(_Multiply_a622abecaf03b48cb6caf70d8f77fba8_Out_2_Float, float(0), float(1), _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float);
            float _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float;
            Unity_Branch_float(_Property_3905b5031443436e8f7c211a4bd40b96_Out_0_Boolean, _Clamp_95b951cdb7cc8687809004d8a1bb89a9_Out_3_Float, float(1), _Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float);
            float _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float;
            Unity_Remap_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, float2 (0, -1), float2 (0, 1), _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float);
            float _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float;
            Unity_Multiply_float_float(_Branch_d8b1ffc4c260416b97f33be33475daab_Out_3_Float, _Remap_e268cae9342dde868bc88b41c521b020_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float);
            float _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float;
            Unity_Sign_float(_DotProduct_d574e64f07ef9983b891024944f3c58b_Out_2_Float, _Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float);
            float _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float;
            Unity_Remap_float(_Sign_f474915117c1bb81a3a80d6b0b95abd9_Out_1_Float, float2 (-1, 1), float2 (1, 0), _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float);
            float _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float;
            Unity_Lerp_float(_Clamp_7d3e05bfb0104781a1019230de6f7c9a_Out_3_Float, _Multiply_cd423534949386898d7e8c62109a9697_Out_2_Float, _Remap_0f70816788cda9879aa9b31a8465920f_Out_3_Float, _Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float);
            float _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float;
            Unity_Clamp_float(_Lerp_d959140ac5b62582a1e63f732d9919d8_Out_3_Float, float(0), float(1), _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float);
            float _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Clamp_15201875e6a63c8aba55e549a59468e9_Out_3_Float, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float);
            float _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            Unity_Branch_float(_Property_ac02d6f9b2790e81a045c097fdd22bf5_Out_0_Boolean, _Multiply_08c8788f6e0a9d878ff9cf7cf7ca089e_Out_2_Float, _Multiply_e60dfa0b5fb780899db96e3fb34a9e82_Out_2_Float, _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float);
            surface.BaseColor = (_Multiply_d562aca368957f83b86107a4542c7189_Out_2_Vector4.xyz);
            surface.Alpha = _Branch_8bb76d35c1b1158d8554230292e1c1bd_Out_3_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.ObjectSpaceNormal = normalize(mul(output.WorldSpaceNormal, (float3x3) UNITY_MATRIX_M));           // transposed multiplication by inverse matrix to handle normal scale
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.ObjectSpaceViewDirection = TransformWorldToObjectDir(output.WorldSpaceViewDirection);
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    CustomEditorForRenderPipeline "UnityEditor.ShaderGraphUnlitGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    FallBack "Hidden/Shader Graph/FallbackError"
} 