Shader "Shader Graphs/MyTest"
{
    Properties
    { 
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            // RenderType: <None>
            // Queue: <None>
            // DisableBatching: <None>
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalFullscreenSubTarget"
        }
        Pass
        {
            Name "DrawProcedural"
            
            // Render State
            Cull Off
            Blend Off
            ZTest Off
            ZWrite Off
            
            // Debug
            // <None>
            
            // --------------------------------------------------
            // Pass
            
            HLSLPROGRAM
            
            // Pragmas
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            // #pragma enable_d3d11_debug_symbols
            
            /* WARNING: $splice Could not find named fragment 'DotsInstancingOptions' */
            /* WARNING: $splice Could not find named fragment 'HybridV1InjectedBuiltinProperties' */
            
            
            
            
            // custom interpolator pre-include
            /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
            
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/Fullscreen/Includes/FullscreenShaderPass.cs.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
            
            // --------------------------------------------------
            // Structs and Packing
            
            // custom interpolators pre packing
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
            
            struct Attributes
            {
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                uint vertexID : VERTEXID_SEMANTIC;
            };
            struct SurfaceDescriptionInputs
            {
                float3 WorldSpacePosition;
                float4 ScreenPosition;
                float2 NDCPosition;
                float2 PixelPosition;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoord0;
                float4 texCoord1;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
            };
            struct VertexDescriptionInputs
            {
            };
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoord0 : INTERP0;
                float4 texCoord1 : INTERP1;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
            };
            
            PackedVaryings PackVaryings (Varyings input)
            {
                PackedVaryings output;
                ZERO_INITIALIZE(PackedVaryings, output);
                output.positionCS = input.positionCS;
                output.texCoord0.xyzw = input.texCoord0;
                output.texCoord1.xyzw = input.texCoord1;
                #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                return output;
            }
            
            Varyings UnpackVaryings (PackedVaryings input)
            {
                Varyings output;
                output.positionCS = input.positionCS;
                output.texCoord0 = input.texCoord0.xyzw;
                output.texCoord1 = input.texCoord1.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                return output;
            }
            
            
            // --------------------------------------------------
            // Graph
            
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END
            
            
            // Object and Global properties
            float _FlipY;
            
            // Graph Includes
            // GraphIncludes: <None>
            
            // Graph Functions
            
            TEXTURE2D_X(_BlitTexture); 
            void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }
            
            

            // Custom interpolators pre vertex
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
            
            // Graph Vertex
            // GraphVertex: <None>
            
            // Custom interpolators, pre surface
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreSurface' */
            
            // Graph Pixel
            struct SurfaceDescription
            {
                float3 BaseColor;
                float Alpha;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                  uint2 pixelCoords = uint2(IN.NDCPosition.xy * _ScreenSize.xy);
                 float4 sourceColor= LOAD_TEXTURE2D_X_LOD(_BlitTexture, pixelCoords, 0);
                 float4 _ResultColor=sourceColor*float4(1,0,0,1); 
                surface.BaseColor = _ResultColor;
                surface.Alpha = 1;
                return surface;
            }
            
            // --------------------------------------------------
            // Build Graph Inputs
            
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                float3 normalWS = SHADERGRAPH_SAMPLE_SCENE_NORMAL(input.texCoord0.xy);
                float4 tangentWS = float4(0, 1, 0, 0); // We can't access the tangent in screen space
                
                
                
                
                float3 viewDirWS = normalize(input.texCoord1.xyz);
                float linearDepth = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(input.texCoord0.xy), _ZBufferParams);
                float3 cameraForward = -UNITY_MATRIX_V[2].xyz;
                float camearDistance = linearDepth / dot(viewDirWS, cameraForward);
                float3 positionWS = viewDirWS * camearDistance + GetCameraPositionWS();
                
                
                output.WorldSpacePosition = positionWS;
                output.ScreenPosition = float4(input.texCoord0.xy, 0, 1);
                output.NDCPosition = input.texCoord0.xy;
                
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                    #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                
                return output;
            }

            


            float4 GetDrawProceduralVertexPosition(uint vertexID)
            {
                return GetFullScreenTriangleVertexPosition(vertexID, UNITY_NEAR_CLIP_VALUE);
            }
            PackedVaryings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = GetDrawProceduralVertexPosition(input.vertexID);

                output.texCoord0 = output.positionCS * 0.5 + 0.5;

                #if UNITY_UV_STARTS_AT_TOP
                    if (_FlipY < 0.5)
                    output.texCoord0.y = 1 - output.texCoord0.y;
                #endif

                PackedVaryings packedOutput = PackVaryings(output);
                return packedOutput;
            }

            struct FragOutput
            {
                float4 color : SV_TARGET;
                #ifdef DEPTH_WRITE
                    float depth : DEPTH_OFFSET_SEMANTIC;
                #endif
            };

            
            FragOutput DefaultFullscreenFragmentShader(PackedVaryings packedInput)
            {
                FragOutput output = (FragOutput)0;
                Varyings unpacked = UnpackVaryings(packedInput);

                UNITY_SETUP_INSTANCE_ID(unpacked);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(unpacked);

                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(unpacked);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);

                output.color.rgb = surfaceDescription.BaseColor;
                output.color.a = 1;
                

                return output;
            }

            FragOutput frag(PackedVaryings packedInput)
            {
                return DefaultFullscreenFragmentShader(packedInput);
            }
            // --------------------------------------------------
            // Main
            
            
            ENDHLSL
        }
        
    }
    CustomEditor "UnityEditor.Rendering.Fullscreen.ShaderGraph.FullscreenShaderGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}