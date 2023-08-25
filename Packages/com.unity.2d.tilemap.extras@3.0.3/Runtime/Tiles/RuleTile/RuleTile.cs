using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;

namespace UnityEngine
{
    public enum RuleType
    {
        AND,OR
    }

    /// <summary>
    /// Generic visual tile for creating different tilesets like terrain, pipeline, random or animated tiles.
    /// This is templated to accept a Neighbor Rule Class for Custom Rules.
    /// </summary>
    /// <typeparam name="T">Neighbor Rule Class for Custom Rules</typeparam>
    public class RuleTile<T> : RuleTile
    {
        /// <summary>
        /// Returns the Neighbor Rule Class type for this Rule Tile.
        /// </summary>
        public sealed override Type m_NeighborType => typeof(T);
    }

    /// <summary>
    /// Generic visual tile for creating different tilesets like terrain, pipeline, random or animated tiles.
    /// </summary>
    [Serializable]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.tilemap.extras@latest/index.html?subfolder=/manual/RuleTile.html")]
    public class RuleTile : TileBase
    {
        /// <summary>
        /// Returns the default Neighbor Rule Class type.
        /// </summary>
        public virtual Type m_NeighborType => typeof(TilingRuleOutput.Neighbor);

        public int m_LayerOrder = 0;
        /// <summary>
        /// The Default Sprite set when creating a new Rule.
        /// </summary>
        public Sprite m_DefaultSprite;
        /// <summary>
        /// The Default GameObject set when creating a new Rule.
        /// </summary>
        public GameObject m_DefaultGameObject;
        /// <summary>
        /// The Default Collider Type set when creating a new Rule.
        /// </summary>
        public Tile.ColliderType m_DefaultColliderType = Tile.ColliderType.Sprite;

        /// <summary>
        /// Angle in which the RuleTile is rotated by for matching in Degrees.
        /// </summary>
        public virtual int m_RotationAngle => 90;

        /// <summary>
        /// Number of rotations the RuleTile can be rotated by for matching.
        /// </summary>
        public int m_RotationCount => 360 / m_RotationAngle;

        /// <summary>
        /// The data structure holding the Rule information for matching Rule Tiles with
        /// its neighbors.
        /// </summary>
        [Serializable]
        public class TilingRuleOutput
        {
            /// <summary>
            /// Id for this Rule.
            /// </summary>
            public int m_Id;

            public int row;
            public int col;
            /// <summary>
            /// The output Sprites for this Rule.
            /// </summary>
            public Sprite[] m_Sprites = new Sprite[1];
            /// <summary>
            /// The output TileBase for this Rule.
            /// </summary>
            public TileBase m_TileBase;

            /// <summary>
            /// The output RuleType for this Rule.
            /// </summary>
            public RuleType m_RuleType;
            /// <summary>
            /// The output GameObject for this Rule.
            /// </summary>
            public GameObject m_GameObject;
            /// <summary>
            /// The output minimum Animation Speed for this Rule.
            /// </summary>
            [FormerlySerializedAs("m_AnimationSpeed")]
            public float m_MinAnimationSpeed = 1f;
            /// <summary>
            /// The output maximum Animation Speed for this Rule.
            /// </summary>
            [FormerlySerializedAs("m_AnimationSpeed")]
            public float m_MaxAnimationSpeed = 1f;
            /// <summary>
            /// The perlin scale factor for this Rule.
            /// </summary>
            public float m_PerlinScale = 0.5f;
            /// <summary>
            /// The output type for this Rule.
            /// </summary>
            public OutputSprite m_Output = OutputSprite.Single;
            /// <summary>
            /// The output Collider Type for this Rule.
            /// </summary>
            public Tile.ColliderType m_ColliderType = Tile.ColliderType.Sprite;
            /// <summary>
            /// The randomized transform output for this Rule.
            /// </summary>
            public Transform m_RandomTransform;

            /// <summary>
            /// The enumeration for matching Neighbors when matching Rule Tiles
            /// </summary>
            public class Neighbor
            {
                /// <summary>
                /// The Rule Tile will check if the contents of the cell in that direction is an instance of this Rule Tile.
                /// If not, the rule will fail.
                /// </summary>
                public const int This = 1;
                /// <summary>
                /// The Rule Tile will check if the contents of the cell in that direction is not an instance of this Rule Tile.
                /// If it is, the rule will fail.
                /// </summary>
                public const int NotThis = 2;
                public const int IsTarget = 3;
                public const int NotTarget = 4;
                public const int NotThisAndNotNull = 5;
                public const int NotTargetAndNotNull = 6;
                public const int NotThisTarget = 7;
                public const int NotThisTargetNull = 8;
                public const int ThisOrTarget = 9;
            }

            /// <summary>
            /// The enumeration for the transform rule used when matching Rule Tiles.
            /// </summary>
            public enum Transform
            {
                /// <summary>
                /// The Rule Tile will match Tiles exactly as laid out in its neighbors.
                /// </summary>
                Fixed,
                /// <summary>
                /// The Rule Tile will rotate and match its neighbors.
                /// </summary>
                Rotated,
                /// <summary>
                /// The Rule Tile will mirror in the X axis and match its neighbors.
                /// </summary>
                MirrorX,
                /// <summary>
                /// The Rule Tile will mirror in the Y axis and match its neighbors.
                /// </summary>
                MirrorY,
                /// <summary>
                /// The Rule Tile will mirror in the X or Y axis and match its neighbors.
                /// </summary>
                MirrorXY
            }

            /// <summary>
            /// The Output for the Tile which fits this Rule.
            /// </summary>
            public enum OutputSprite
            {
               
                /// <summary>
                /// A Single Sprite will be output.
                /// </summary>
                Single,
                /// <summary>
                /// A Random Sprite will be output.
                /// </summary>
                Random,
                /// <summary>
                /// A Sprite Animation will be output.
                /// </summary>
                Animation,
                Sequence,
                FixedSequeceSingle,
            }
        }

        /// <summary>
        /// The data structure holding the Rule information for matching Rule Tiles with
        /// its neighbors.
        /// </summary>
        [Serializable]
        public class TilingRule : TilingRuleOutput
        { 
            /// <summary>
            /// The matching Rule conditions for each of its neighboring Tiles.
            /// </summary>
            public List<int> m_Neighbors = new List<int>();
            /// <summary>
            /// * Preset this list to RuleTile backward compatible, but not support for HexagonalRuleTile backward compatible.
            /// </summary>
            public List<Vector3Int> m_NeighborPositions = new List<Vector3Int>()
            {
                new Vector3Int(-1, 1, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(1, 1, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(1, 0, 0),
                new Vector3Int(-1, -1, 0),
                new Vector3Int(0, -1, 0),
                new Vector3Int(1, -1, 0),
            };
            /// <summary>
            /// The transform matching Rule for this Rule.
            /// </summary>
            public Transform m_RuleTransform;

           
            /// <summary>
            /// This clones a copy of the TilingRule.
            /// </summary>
            /// <returns>A copy of the TilingRule.</returns>
            public TilingRule Clone()
            {
                TilingRule rule = new TilingRule
                {
                    m_Neighbors = new List<int>(m_Neighbors),
                    m_NeighborPositions = new List<Vector3Int>(m_NeighborPositions),
                    m_RuleTransform = m_RuleTransform,
                    m_Sprites = new Sprite[m_Sprites.Length],
                    m_GameObject = m_GameObject,
                    m_MinAnimationSpeed = m_MinAnimationSpeed,
                    m_MaxAnimationSpeed = m_MaxAnimationSpeed,
                    m_PerlinScale = m_PerlinScale,
                    m_Output = m_Output,
                    m_ColliderType = m_ColliderType,
                    m_RandomTransform = m_RandomTransform, 

                    m_TileBase = m_TileBase,
                    row = row,
                    col=col,
                };
                Array.Copy(m_Sprites, rule.m_Sprites, m_Sprites.Length);
                return rule;
            }
            
            /// <summary>
            /// Returns all neighbors of this Tile as a dictionary
            /// </summary>
            /// <returns>A dictionary of neighbors for this Tile</returns>
            public Dictionary<Vector3Int, int> GetNeighbors()
            {
                Dictionary<Vector3Int, int> dict = new Dictionary<Vector3Int, int>();

                for (int i = 0; i < m_Neighbors.Count && i < m_NeighborPositions.Count; i++)
                    dict.Add(m_NeighborPositions[i], m_Neighbors[i]);

                return dict;
            }

            /// <summary>
            /// Applies the values from the given dictionary as this Tile's neighbors
            /// </summary>
            /// <param name="dict">Dictionary to apply values from</param>
            public void ApplyNeighbors(Dictionary<Vector3Int, int> dict)
            {
                m_NeighborPositions = dict.Keys.ToList();
                m_Neighbors = dict.Values.ToList();
            }

            /// <summary>
            /// Gets the cell bounds of the TilingRule.
            /// </summary>
            /// <returns>Returns the cell bounds of the TilingRule.</returns>
            public BoundsInt GetBounds()
            {
                BoundsInt bounds = new BoundsInt(Vector3Int.zero, Vector3Int.one);
                foreach (var neighbor in GetNeighbors())
                {
                    bounds.xMin = Mathf.Min(bounds.xMin, neighbor.Key.x);
                    bounds.yMin = Mathf.Min(bounds.yMin, neighbor.Key.y);
                    bounds.xMax = Mathf.Max(bounds.xMax, neighbor.Key.x + 1);
                    bounds.yMax = Mathf.Max(bounds.yMax, neighbor.Key.y + 1);
                }
                return bounds;
            }
        }

        /// <summary>
        /// Attribute which marks a property which cannot be overridden by a RuleOverrideTile
        /// </summary>
        public class DontOverride : Attribute { }

        /// <summary>
        /// A list of Tiling Rules for the Rule Tile.
        /// </summary>
        /// 
       
        [HideInInspector]
        public List<TilingRule> m_TilingRules = new List<RuleTile.TilingRule>();

        
        //public Dictionary<TileBase, List<TilingRule>> TilingRules = new Dictionary<TileBase, List<TilingRule>>();

        /// <summary>
        /// Returns a set of neighboring positions for this RuleTile
        /// </summary>
        public HashSet<Vector3Int> neighborPositions
        {
            get
            {
                if (m_NeighborPositions.Count == 0)
                    UpdateNeighborPositions();

                return m_NeighborPositions;
            }
        }

        private HashSet<Vector3Int> m_NeighborPositions = new HashSet<Vector3Int>();

        /// <summary>
        /// Updates the neighboring positions of this RuleTile
        /// </summary>
        public void UpdateNeighborPositions()
        {
            m_CacheTilemapsNeighborPositions.Clear();

            HashSet<Vector3Int> positions = m_NeighborPositions;
            positions.Clear();

            foreach (TilingRule rule in m_TilingRules)
            {
                foreach (var neighbor in rule.GetNeighbors())
                {
                    Vector3Int position = neighbor.Key;
                    positions.Add(position);

                    // Check rule against rotations of 0, 90, 180, 270
                    if (rule.m_RuleTransform == TilingRuleOutput.Transform.Rotated)
                    {
                        for (int angle = m_RotationAngle; angle < 360; angle += m_RotationAngle)
                        {
                            positions.Add(GetRotatedPosition(position, angle));
                        }
                    }
                    // Check rule against x-axis, y-axis mirror
                    else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorXY)
                    {
                        positions.Add(GetMirroredPosition(position, true, true));
                        positions.Add(GetMirroredPosition(position, true, false));
                        positions.Add(GetMirroredPosition(position, false, true));
                    }
                    // Check rule against x-axis mirror
                    else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorX)
                    {
                        positions.Add(GetMirroredPosition(position, true, false));
                    }
                    // Check rule against y-axis mirror
                    else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorY)
                    {
                        positions.Add(GetMirroredPosition(position, false, true));
                    }
                }
            }
        }

        /// <summary>
        /// StartUp is called on the first frame of the running Scene.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="instantiatedGameObject">The GameObject instantiated for the Tile.</param>
        /// <returns>Whether StartUp was successful</returns>
        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instantiatedGameObject)
        {
            if (instantiatedGameObject != null)
            {
                Tilemap tmpMap = tilemap.GetComponent<Tilemap>();
                Matrix4x4 orientMatrix = tmpMap.orientationMatrix;

                var iden = Matrix4x4.identity;
                Vector3 gameObjectTranslation = new Vector3();
                Quaternion gameObjectRotation = new Quaternion();
                Vector3 gameObjectScale = new Vector3();

                bool ruleMatched = false;
                Matrix4x4 transform = iden;

                Vector2Int index = Vector2Int.zero;
                foreach (TilingRule rule in m_TilingRules)
                {
                    if (RuleMatches(rule, position, tilemap, ref transform, ref index))
                    {
                        transform = orientMatrix * transform;

                        // Converts the tile's translation, rotation, & scale matrix to values to be used by the instantiated GameObject
                        gameObjectTranslation = new Vector3(transform.m03, transform.m13, m_LayerOrder*0.01f);
                        gameObjectRotation = Quaternion.LookRotation(new Vector3(transform.m02, transform.m12, transform.m22), new Vector3(transform.m01, transform.m11, transform.m21));
                        gameObjectScale = transform.lossyScale;

                        ruleMatched = true;
                        break;
                    }
                }
                if (!ruleMatched)
                {
                    // Fallback to just using the orientMatrix for the translation, rotation, & scale values.
                    gameObjectTranslation = new Vector3(orientMatrix.m03, orientMatrix.m13, m_LayerOrder * 0.01f);
                    gameObjectRotation = Quaternion.LookRotation(new Vector3(orientMatrix.m02, orientMatrix.m12, orientMatrix.m22), new Vector3(orientMatrix.m01, orientMatrix.m11, orientMatrix.m21));
                    gameObjectScale = orientMatrix.lossyScale;
                }

                instantiatedGameObject.transform.localPosition = gameObjectTranslation + tmpMap.CellToLocalInterpolated(position + tmpMap.tileAnchor)+
                    new Vector3(0,0, m_LayerOrder * 0.01f);
                instantiatedGameObject.transform.localRotation = gameObjectRotation;
                instantiatedGameObject.transform.localScale = gameObjectScale;
            }

            return true;
        }

        /// <summary>
        /// Retrieves any tile rendering data from the scripted tile.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="tileData">Data to render the tile.</param>
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            float z = m_LayerOrder * 0.01f;
            //Debug.Log($"GetTileData:{position}");
            var iden = Matrix4x4.TRS(new Vector3(0,0,z), Quaternion.Euler(0f, 0f, 0f), Vector3.one); ;


            tileData.sprite = m_DefaultSprite;
            tileData.gameObject = m_DefaultGameObject;
            tileData.colliderType = m_DefaultColliderType;
            tileData.flags = TileFlags.LockTransform;
            tileData.transform = iden;

            Matrix4x4 transform = iden;
            foreach (TilingRule rule in m_TilingRules)
            {

                if (rule.m_Sprites.Length == 0)
                {
                    rule.m_Sprites = new Sprite[1]
                    {
                        m_DefaultSprite
                    };
                }

                Vector2Int sequenceIndex = Vector2Int.zero;
                if (RuleMatches(rule, position, tilemap, ref transform,ref sequenceIndex))
                {
                   // Debug.Log("Match!!");
                    switch (rule.m_Output)
                    {
                        case TilingRuleOutput.OutputSprite.Single:
                        case TilingRuleOutput.OutputSprite.Animation:
                            tileData.sprite = rule.m_Sprites[0];
                            break;
                        case TilingRuleOutput.OutputSprite.Random:
                            int index = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, rule.m_PerlinScale, 100000f) * rule.m_Sprites.Length), 0, rule.m_Sprites.Length - 1);
                            tileData.sprite = rule.m_Sprites[index];
                            if (rule.m_RandomTransform != TilingRuleOutput.Transform.Fixed)
                                transform = ApplyRandomTransform(rule.m_RandomTransform, transform, rule.m_PerlinScale, position);
                            break;
                        case TilingRuleOutput.OutputSprite.Sequence:
                            int _index = rule.row * sequenceIndex.y + sequenceIndex.x;
                            if (rule.m_Sprites.Length > _index)
                            {
                                tileData.sprite = rule.m_Sprites[_index];
                            }
                            else
                            {
                                tileData.sprite = rule.m_Sprites[0];
                            }
                           // Debug.Log($"sequenceIndex:{sequenceIndex},_index:{_index},rule.m_Sprites.Length{rule.m_Sprites.Length}");
                            transform.m30 = sequenceIndex.x;
                            transform.m31 = sequenceIndex.y;
                            break;
                        case TilingRuleOutput.OutputSprite.FixedSequeceSingle: 
                            tileData.sprite = rule.m_Sprites[0];
                            // Debug.Log($"sequenceIndex:{sequenceIndex},_index:{_index},rule.m_Sprites.Length{rule.m_Sprites.Length}");
                            transform.m30 = rule.row;
                            transform.m31 = rule.col;
                            break;
                    }
                    //transform.m32 = z;
                    tileData.transform = transform;
                    tileData.gameObject = rule.m_GameObject;
                    tileData.colliderType = rule.m_ColliderType;
                    break;
                }
            }
        }

        /// <summary>
        /// Returns a Perlin Noise value based on the given inputs.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="scale">The Perlin Scale factor of the Tile.</param>
        /// <param name="offset">Offset of the Tile on the Tilemap.</param>
        /// <returns>A Perlin Noise value based on the given inputs.</returns>
        public static float GetPerlinValue(Vector3Int position, float scale, float offset)
        {
            return Mathf.PerlinNoise((position.x + offset) * scale, (position.y + offset) * scale);
        }

        static Dictionary<Tilemap, KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>> m_CacheTilemapsNeighborPositions = new Dictionary<Tilemap, KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>>();
        static TileBase[] m_AllocatedUsedTileArr = Array.Empty<TileBase>();

        static bool IsTilemapUsedTilesChange(Tilemap tilemap, out KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>> hashSet)
        {
            if (!m_CacheTilemapsNeighborPositions.TryGetValue(tilemap, out hashSet))
                return true;

            var oldUsedTiles = hashSet.Key;
            int newUsedTilesCount = tilemap.GetUsedTilesCount();
            if (newUsedTilesCount != oldUsedTiles.Count)
                return true;

            if (m_AllocatedUsedTileArr.Length < newUsedTilesCount)
                Array.Resize(ref m_AllocatedUsedTileArr, newUsedTilesCount);

            tilemap.GetUsedTilesNonAlloc(m_AllocatedUsedTileArr);
            for (int i = 0; i < newUsedTilesCount; i++)
            {
                TileBase newUsedTile = m_AllocatedUsedTileArr[i];
                if (!oldUsedTiles.Contains(newUsedTile))
                    return true;
            }

            return false;
        }

        static KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>> CachingTilemapNeighborPositions(Tilemap tilemap)
        {
            int usedTileCount = tilemap.GetUsedTilesCount();
            HashSet<TileBase> usedTiles = new HashSet<TileBase>();
            HashSet<Vector3Int> neighborPositions = new HashSet<Vector3Int>();

            if (m_AllocatedUsedTileArr.Length < usedTileCount)
                Array.Resize(ref m_AllocatedUsedTileArr, usedTileCount);

            tilemap.GetUsedTilesNonAlloc(m_AllocatedUsedTileArr);

            for (int i = 0; i < usedTileCount; i++)
            {
                TileBase tile = m_AllocatedUsedTileArr[i];
                usedTiles.Add(tile);
                RuleTile ruleTile = null;

                if (tile is RuleTile rt)
                    ruleTile = rt;
                else if (tile is RuleOverrideTile ot)
                    ruleTile = ot.m_Tile;

                if (ruleTile)
                    foreach (Vector3Int neighborPosition in ruleTile.neighborPositions)
                        neighborPositions.Add(neighborPosition);
            }

            var value = new KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>(usedTiles, neighborPositions);
            m_CacheTilemapsNeighborPositions[tilemap] = value;
            return value;
        }

        static bool NeedRelease()
        {
            foreach (var keypair in m_CacheTilemapsNeighborPositions)
            {
                if (keypair.Key == null)
                {
                    return true;
                }
            }
            return false;
        }
        
        static void ReleaseDestroyedTilemapCacheData()
        {
            if (!NeedRelease())
                return;

            var hasCleared = false;
            var keys = m_CacheTilemapsNeighborPositions.Keys.ToArray();
            foreach (var key in keys)
            {
                if (key == null && m_CacheTilemapsNeighborPositions.Remove(key))
                    hasCleared = true;
            }
            if (hasCleared)
            {
                // TrimExcess
                m_CacheTilemapsNeighborPositions = new Dictionary<Tilemap, KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>>(m_CacheTilemapsNeighborPositions);
            }
        }

        /// <summary>
        /// Retrieves any tile animation data from the scripted tile.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="tileAnimationData">Data to run an animation on the tile.</param>
        /// <returns>Whether the call was successful.</returns>
        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            Matrix4x4 transform = Matrix4x4.identity;
            Vector2Int index = Vector2Int.zero;
            foreach (TilingRule rule in m_TilingRules)
            {
                if (rule.m_Output == TilingRuleOutput.OutputSprite.Animation)
                {
                    if (RuleMatches(rule, position, tilemap, ref transform,ref index))
                    {
                        tileAnimationData.animatedSprites = rule.m_Sprites;
                        tileAnimationData.animationSpeed = Random.Range( rule.m_MinAnimationSpeed, rule.m_MaxAnimationSpeed);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This method is called when the tile is refreshed.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            base.RefreshTile(position, tilemap);

            Tilemap baseTilemap = tilemap.GetComponent<Tilemap>();

            ReleaseDestroyedTilemapCacheData(); // Prevent memory leak

            if (IsTilemapUsedTilesChange(baseTilemap, out var neighborPositionsSet))
                neighborPositionsSet = CachingTilemapNeighborPositions(baseTilemap);

            var neighborPositionsRuleTile = neighborPositionsSet.Value;
            foreach (Vector3Int offset in neighborPositionsRuleTile)
            {
                Vector3Int offsetPosition = GetOffsetPositionReverse(position, offset);
                TileBase tile = tilemap.GetTile(offsetPosition);
                RuleTile ruleTile = null;

                if (tile is RuleTile rt)
                    ruleTile = rt;
                else if (tile is RuleOverrideTile ot)
                    ruleTile = ot.m_Tile;

                if (ruleTile != null)
                    if (ruleTile == this || ruleTile.neighborPositions.Contains(offset))
                        base.RefreshTile(offsetPosition, tilemap);
            }
        }

        /// <summary>
        /// Does a Rule Match given a Tiling Rule and neighboring Tiles.
        /// </summary>
        /// <param name="rule">The Tiling Rule to match with.</param>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The tilemap to match with.</param>
        /// <param name="transform">A transform matrix which will match the Rule.</param>
        /// <returns>True if there is a match, False if not.</returns>
        public virtual bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, ref Matrix4x4 transform,ref Vector2Int sequenceIndex)
        {
            float z = m_LayerOrder*0.01f;
            sequenceIndex = Vector2Int.zero;
            if (RuleMatches(rule, position, tilemap, 0,ref sequenceIndex))
            {
                transform = Matrix4x4.TRS(new Vector3(0,0,z), Quaternion.Euler(0f, 0f, 0f), Vector3.one);
                return true;
            }

            // Check rule against rotations of 0, 90, 180, 270
            if (rule.m_RuleTransform == TilingRuleOutput.Transform.Rotated)
            {
                for (int angle = m_RotationAngle; angle < 360; angle += m_RotationAngle)
                {
                    if (RuleMatches(rule, position, tilemap, angle, ref sequenceIndex))
                    {
                        transform = Matrix4x4.TRS(new Vector3(0, 0, z), Quaternion.Euler(0f, 0f, -angle), Vector3.one);
                        return true;
                    }
                }
            }
            // Check rule against x-axis, y-axis mirror
            else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorXY)
            {
                if (RuleMatches(rule, position, tilemap, true, true))
                {
                    transform = Matrix4x4.TRS(new Vector3(0, 0, z), Quaternion.identity, new Vector3(-1f, -1f, 1f));
                    return true;
                }
                if (RuleMatches(rule, position, tilemap, true, false))
                {
                    transform = Matrix4x4.TRS(new Vector3(0, 0, z), Quaternion.identity, new Vector3(-1f, 1f, 1f));
                    return true;
                }
                if (RuleMatches(rule, position, tilemap, false, true))
                {
                    transform = Matrix4x4.TRS(new Vector3(0, 0, z), Quaternion.identity, new Vector3(1f, -1f, 1f));
                    return true;
                }
            }
            // Check rule against x-axis mirror
            else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorX)
            {
                if (RuleMatches(rule, position, tilemap, true, false))
                {
                    transform = Matrix4x4.TRS(new Vector3(0, 0, z), Quaternion.identity, new Vector3(-1f, 1f, 1f));
                    return true;
                }
            }
            // Check rule against y-axis mirror
            else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorY)
            {
                if (RuleMatches(rule, position, tilemap, false, true))
                {
                    transform = Matrix4x4.TRS(new Vector3(0, 0, z), Quaternion.identity, new Vector3(1f, -1f, 1f));
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a random transform matrix given the random transform rule.
        /// </summary>
        /// <param name="type">Random transform rule.</param>
        /// <param name="original">The original transform matrix.</param>
        /// <param name="perlinScale">The Perlin Scale factor of the Tile.</param>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <returns>A random transform matrix.</returns>
        public virtual Matrix4x4 ApplyRandomTransform(TilingRuleOutput.Transform type, Matrix4x4 original, float perlinScale, Vector3Int position)
        {
            float z = m_LayerOrder*0.01f;
            float perlin = GetPerlinValue(position, perlinScale, 200000f);
            switch (type)
            {
                case TilingRuleOutput.Transform.MirrorXY:
                    original= original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Math.Abs(perlin - 0.5) > 0.25 ? 1f : -1f, perlin < 0.5 ? 1f : -1f, 1f));
                    break;
                case TilingRuleOutput.Transform.MirrorX:
                    original= original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(perlin < 0.5 ? 1f : -1f, 1f, 1f));
                    break;
                case TilingRuleOutput.Transform.MirrorY:
                     original= original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, perlin < 0.5 ? 1f : -1f, 1f));
                    break;
                case TilingRuleOutput.Transform.Rotated:
                    int angle = Mathf.Clamp(Mathf.FloorToInt(perlin * m_RotationCount), 0, m_RotationCount - 1) * m_RotationAngle;
                    original = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -angle), Vector3.one);
                    break;
            }
            var pos = original.GetPosition();
            var rot = original.rotation;
            var scale = original.lossyScale;

            pos.z = z;
            original.SetTRS(pos, rot, scale);
            //original.m32 = z;
            return original;
        }

        /// <summary>
        /// Returns custom fields for this RuleTile
        /// </summary>
        /// <param name="isOverrideInstance">Whether override fields are returned</param>
        /// <returns>Custom fields for this RuleTile</returns>
        public FieldInfo[] GetCustomFields(bool isOverrideInstance)
        {
            return this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => typeof(RuleTile).GetField(field.Name) == null)
                .Where(field => field.IsPublic || field.IsDefined(typeof(SerializeField)))
                .Where(field => !field.IsDefined(typeof(HideInInspector)))
                .Where(field => !isOverrideInstance || !field.IsDefined(typeof(DontOverride)))
                .ToArray();
        }

        /// <summary>
        /// Checks if there is a match given the neighbor matching rule and a Tile.
        /// </summary>
        /// <param name="neighbor">Neighbor matching rule.</param>
        /// <param name="other">Tile to match.</param>
        /// <returns>True if there is a match, False if not.</returns>
        public virtual bool RuleMatch(int neighbor, TileBase other,TileBase tileBase)
        { 
            TileBase InstanceTile = other;
            if (other is RuleOverrideTile ot)
            {
                InstanceTile = ot.m_InstanceTile;
                //other = ot.m_InstanceTile;
            }
               
            if (tileBase == null)
            {
                switch (neighbor)
                {
                    case TilingRuleOutput.Neighbor.This:
                        return InstanceTile == this;
                    case TilingRuleOutput.Neighbor.IsTarget:
                        return other == this;
                    case TilingRuleOutput.Neighbor.ThisOrTarget:
                        return other == this|| InstanceTile == this;
                    case TilingRuleOutput.Neighbor.NotTarget:
                        return other != this;
                    case TilingRuleOutput.Neighbor.NotThis:
                        return InstanceTile != this;
                    case TilingRuleOutput.Neighbor.NotThisTarget:
                        return other != this&& InstanceTile != this;
                    case TilingRuleOutput.Neighbor.NotThisAndNotNull:
                        return InstanceTile != this && InstanceTile !=null;
                    case TilingRuleOutput.Neighbor.NotTargetAndNotNull:
                    case TilingRuleOutput.Neighbor.NotThisTargetNull:
                        return other != this && other != null && InstanceTile != null;
                }
            }
            else
            {
                switch (neighbor)
                {
                    case TilingRuleOutput.Neighbor.This: 
                        return InstanceTile == this|| other == this;
                    case TilingRuleOutput.Neighbor.NotThis: 
                        return InstanceTile != this&& other != this;
                    case TilingRuleOutput.Neighbor.IsTarget:
                        return InstanceTile == tileBase|| other == tileBase;
                    case TilingRuleOutput.Neighbor.NotTarget:
                        return InstanceTile != tileBase && other != tileBase;
                    case TilingRuleOutput.Neighbor.NotTargetAndNotNull:
                        return InstanceTile != tileBase&& InstanceTile != null&&
                            other != tileBase && other != null;
                    case TilingRuleOutput.Neighbor.NotThisTarget:
                        return InstanceTile != this && InstanceTile != tileBase&& other != this && other != tileBase;
                    case TilingRuleOutput.Neighbor.NotThisAndNotNull:
                        return InstanceTile != this && InstanceTile != null&& other != this && other != null;
                    case TilingRuleOutput.Neighbor.NotThisTargetNull:
                        return InstanceTile != this&& InstanceTile != tileBase && InstanceTile != null&&
                            other != this && other != tileBase && other != null;
                    case TilingRuleOutput.Neighbor.ThisOrTarget:
                        return InstanceTile == this || InstanceTile == tileBase||
                            other == this || other == tileBase;
                }
            }
          
            return true;
        }


       static List<Vector3Int> _NeighborPositions = new List<Vector3Int>()
        {
                new Vector3Int(-1, 1, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(1, 1, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(1, 0, 0),
                new Vector3Int(-1, -1, 0),
                new Vector3Int(0, -1, 0),
                new Vector3Int(1, -1, 0),
       };

        /// <summary>
        /// Checks if there is a match given the neighbor matching rule and a Tile with a rotation angle.
        /// </summary>
        /// <param name="rule">Neighbor matching rule.</param>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">Tilemap to match.</param>
        /// <param name="angle">Rotation angle for matching.</param>
        /// <returns>True if there is a match, False if not.</returns>
        public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, int angle,
            ref Vector2Int sequenceIndex)
        {

            if (rule.m_Output == TilingRuleOutput.OutputSprite.Sequence)
            {
                List<Vector2Int> indexs = new List<Vector2Int>();
                for (int i = 0; i < _NeighborPositions.Count; i++)
                {
                    Vector3Int positionOffset = GetRotatedPosition(_NeighborPositions[i], angle);
                    Vector3Int _positionOffset = GetOffsetPosition(position, positionOffset);

                    TileBase other = tilemap.GetTile(_positionOffset);
                    if (other != this)
                    {
                        indexs.Add(new Vector2Int(-1, -1));
                    }
                    else
                    {
                        //Debug.Log($"other:{other.name}positionOffset:{_positionOffset};myPos:{position}");

                        var Matrix = tilemap.GetTransformMatrix(_positionOffset);
                        //var scale = Matrix.lossyScale;
                        indexs.Add(new Vector2Int((int)Matrix.m30, (int)Matrix.m31));
                    }
                }
                int x = 0;
                int y = 0;
                if (indexs[3].x != -1)
                {
                    x= indexs[3].x +1;

                    if (x > rule.row - 1)
                    {
                        x = 0;
                    }
                    else
                    {
                        y = indexs[3].y;

                       // Debug.Log($"indexs[3]:{y}");
                    }
                }
                if (x == 0&&indexs[1].y != -1)
                {
                    y=indexs[1].y-1;

                    if (y < 0)
                    {
                        y = rule.col - 1;
                    }
                    else
                    {
                        x = indexs[1].x;
                    }
                }
                /*if (x == 0&& y == 0)
                {
                    if (indexs[4].x != -1)
                    {
                        x--;
                        if (x < 0)
                        {
                            x = rule.row - 1;
                        }
                        else
                        {
                            y = indexs[4].y;
                        }
                    }
                }

                if (x == 0 && y == 0)
                {
                    if (indexs[6].y != -1)
                    {
                        y--;
                        if (y < 0)
                        {
                            y = rule.row - 1;
                        }
                        else
                        {
                            x = indexs[6].x;
                        }
                    }
                }*/

                sequenceIndex=new Vector2Int(x,y);
                //Debug.Log($"sequenceIndex{sequenceIndex}");
            } 
           




            var minCount = Math.Min(rule.m_Neighbors.Count, rule.m_NeighborPositions.Count);
            for (int i = 0; i < minCount ; i++)
            {
                int neighbor = rule.m_Neighbors[i];
                Vector3Int positionOffset = GetRotatedPosition(rule.m_NeighborPositions[i], angle);
                TileBase other = tilemap.GetTile(GetOffsetPosition(position, positionOffset));
                bool match = RuleMatch(neighbor, other, rule.m_TileBase);
                if (rule.m_RuleType == RuleType.AND&&!match)
                {
                    return false;
                }
                if (rule.m_RuleType == RuleType.OR && match)
                {
                    return true;
                }
            }
            if (rule.m_RuleType == RuleType.AND)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if there is a match given the neighbor matching rule and a Tile with mirrored axii.
        /// </summary>
        /// <param name="rule">Neighbor matching rule.</param>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">Tilemap to match.</param>
        /// <param name="mirrorX">Mirror X Axis for matching.</param>
        /// <param name="mirrorY">Mirror Y Axis for matching.</param>
        /// <returns>True if there is a match, False if not.</returns>
        public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, bool mirrorX, bool mirrorY)
        {
            var minCount = Math.Min(rule.m_Neighbors.Count, rule.m_NeighborPositions.Count);
            for (int i = 0; i < minCount; i++)
            {
                int neighbor = rule.m_Neighbors[i];
                Vector3Int positionOffset = GetMirroredPosition(rule.m_NeighborPositions[i], mirrorX, mirrorY);
                TileBase other = tilemap.GetTile(GetOffsetPosition(position, positionOffset));
                bool match = RuleMatch(neighbor, other, rule.m_TileBase);
                if (rule.m_RuleType == RuleType.AND && !match)
                {
                    return false;
                }
                if (rule.m_RuleType == RuleType.OR && match)
                {
                    return true;
                } 
            }
            if (rule.m_RuleType == RuleType.AND)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a rotated position given its original position and the rotation in degrees. 
        /// </summary>
        /// <param name="position">Original position of Tile.</param>
        /// <param name="rotation">Rotation in degrees.</param>
        /// <returns>Rotated position of Tile.</returns>
        public virtual Vector3Int GetRotatedPosition(Vector3Int position, int rotation)
        {
            switch (rotation)
            {
                case 0:
                    return position;
                case 90:
                    return new Vector3Int(position.y, -position.x, 0);
                case 180:
                    return new Vector3Int(-position.x, -position.y, 0);
                case 270:
                    return new Vector3Int(-position.y, position.x, 0);
            }
            return position;
        }

        /// <summary>
        /// Gets a mirrored position given its original position and the mirroring axii.
        /// </summary>
        /// <param name="position">Original position of Tile.</param>
        /// <param name="mirrorX">Mirror in the X Axis.</param>
        /// <param name="mirrorY">Mirror in the Y Axis.</param>
        /// <returns>Mirrored position of Tile.</returns>
        public virtual Vector3Int GetMirroredPosition(Vector3Int position, bool mirrorX, bool mirrorY)
        {
            if (mirrorX)
                position.x *= -1;
            if (mirrorY)
                position.y *= -1;
            return position;
        }

        /// <summary>
        /// Get the offset for the given position with the given offset.
        /// </summary>
        /// <param name="position">Position to offset.</param>
        /// <param name="offset">Offset for the position.</param>
        /// <returns>The offset position.</returns>
        public virtual Vector3Int GetOffsetPosition(Vector3Int position, Vector3Int offset)
        {
            return position + offset;
        }

        /// <summary>
        /// Get the reversed offset for the given position with the given offset.
        /// </summary>
        /// <param name="position">Position to offset.</param>
        /// <param name="offset">Offset for the position.</param>
        /// <returns>The reversed offset position.</returns>
        public virtual Vector3Int GetOffsetPositionReverse(Vector3Int position, Vector3Int offset)
        {
            return position - offset;
        }
    }
}
