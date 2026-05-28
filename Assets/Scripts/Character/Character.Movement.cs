using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public partial class Character
{
    public void StopMove()
    {
        GameObjectCurveController.instance.StopObjectMove(instanceId);
        if (GameObjectCurveController.instance.Pause(moveHandle))
        {
            CharacterManager.instance.SetCharacterAnimationSpeed(0, this);
        };
    }

    public void RemoveMove()
    {
        GameObjectCurveController.instance.Cancel(moveHandle);
        CharacterManager.instance.SetCharacterAnimationSpeed(0, this);
        moveHandle = default;
        moveState.Clear();
    }

    public void StartMove()
    {
        if (GameObjectCurveController.instance.Resume(moveHandle))
        {
            CharacterManager.instance.SetCharacterAnimationSpeed(1, this);
        }
        // GameController.instance.StopCoroutine(moveEnumerator);
    }

    private CharacterMoveState moveState;
    public int3 moveTarget => moveState.Target;
    public MoveEndAction moveEndAction => moveState.MoveEndAction;
    public MoveEndAction changeCoordinateAction => moveState.ChangeCoordinateAction;
    public Int3Action failedMoveAction => moveState.FailedMoveAction;

    public bool TryMove(int2 targetCoordinate, MoveEndAction moveEndAction = null, MoveEndAction changeCoordinateAction = null,
        Int3Action failedMoveAction = null)
    {
        return TryMove(mapInstance, targetCoordinate, moveEndAction, changeCoordinateAction, failedMoveAction);
    }
    public bool TryMove(int targetMap, int2 targetCoordinate, MoveEndAction moveEndAction = null, MoveEndAction changeCoordinateAction = null,
        Int3Action failedMoveAction = null)
    {
        if (targetMap == 948 && targetCoordinate.x == 0 && targetCoordinate.y == 0) Debug.Log("Error");
        if (targetMap == mapInstance && targetCoordinate.x == coordinate.x && targetCoordinate.y == coordinate.y)
        {
            moveEndAction?.Invoke();
            return true;
        }
        if (!CanMoveCrossMap)
        {
            Debug.Log($"NoCanMoveCrossMap");
            failedMoveAction?.Invoke(new int3(targetCoordinate.xy, targetMap));
            return false;
        }

        moveState.Begin(new int3(targetCoordinate.xy, targetMap), moveEndAction, changeCoordinateAction, failedMoveAction);

        void FailedMoveAction()
        {
            moveState.Fail(new int3(targetCoordinate.xy, targetMap));
        }

        void CompleteMove()
        {
            moveState.Complete();
        }

        void CompleteMoveAfterMapLerp()
        {
            canMove = false;
            GameTimerController.instance.DelayAction((int)(GameCommon.mapChangeLerpTime * 1000), CompleteMove);
        }

        void ChangeCoordinateAction()
        {
            moveState.ChangeCoordinate();
        }

        if (targetMap== objCoordinate.z)
        {
            MapCellJobController.instance.AddPathRequest(objCoordinate.xy, targetCoordinate, targetMap,
                (Stack<int2> path, int map, int2 start, int2 end) =>
            {
                PlayerMove(path, () =>
                {
                    // Debug.Log($"character:{name}--MovePathEnd");
                    if (this == CharacterManager.instance.controllerCharacter)
                    {
                        CompleteMoveAfterMapLerp();
                    }
                    else
                    {
                        CompleteMove();
                    }
                }, ChangeCoordinateAction, FailedMoveAction);
            });

            return true;
        }
        else
        {
            Dictionary<int,Stack<int2>> roadCells = new Dictionary<int,Stack<int2>>();
            Queue<int> roomQueue = new Queue<int>();
            if(MapCellController.instance.FindRoomList(objCoordinate.z, targetMap, out var roomList))
            {
                int nowMap = objCoordinate.z;
                int2 startCoordinate = objCoordinate.xy;
                int roomCount = roomList.Count+1;
                int nextMap= nowMap;
                int2 targetMapCell = int2.zero;
                for (int i = 0; i <= roomList.Count; i++)
                {
                    roomQueue.Enqueue(nowMap);
                    if (i < roomList.Count)
                    {
                        nextMap = roomList[i];
                        var nowCoordinate = startCoordinate;
                        var endCoordinate = targetCoordinate;
                        if (i != 0) nowCoordinate = new int2(int.MinValue, int.MinValue);
                        if (i != roomList.Count - 1) endCoordinate = new int2(int.MinValue, int.MinValue);

                        if (MapCellController.instance.GetLinkMapInCoordinate(mapInstance, targetMap,
                                nowMap, nextMap, nowCoordinate, endCoordinate,
                                out var changeCoordinate))
                        {
                            targetMapCell = changeCoordinate.zw;
#if UNITY_EDITOR
                            var mapRange = MapCellController.instance.GetRoomRange(nowMap);
                            if (startCoordinate.x < mapRange.x || startCoordinate.y < mapRange.y ||
                                startCoordinate.x > mapRange.z || startCoordinate.y > mapRange.w)
                                Debug.Log("错误：起始超出地图范围！");

                            if (changeCoordinate.x < mapRange.x || changeCoordinate.y < mapRange.y ||
                                changeCoordinate.x > mapRange.z || changeCoordinate.y > mapRange.w)
                                Debug.Log("错误：目标超出地图范围！");

                            var nextMapRange = MapCellController.instance.GetRoomRange(nextMap);
                            if (targetMapCell.x < nextMapRange.x || targetMapCell.y < nextMapRange.y ||
                                targetMapCell.x > nextMapRange.z || targetMapCell.y > nextMapRange.w)
                                Debug.Log("错误：起始超出地图范围！");


#endif


                            MapCellJobController.instance.AddPathRequest(startCoordinate, changeCoordinate.xy, nowMap,
                                MoveWithPath);

                            void MoveWithPath(Stack<int2> path, int map, int2 start, int2 end)
                            {
                                if (path.Count == 0)
                                    Debug.Log($"PlayerMove：startCoordinate{start}targetCoordinate{end}-nowMap{map}");
                                roadCells.Add(map, path);
                                roomCount--;
                                if (roomCount == 0) Move(true);
                            }
                        }
                    }
                    else
                    {
#if UNITY_EDITOR
                        var mapRange = MapCellController.instance.GetRoomRange(nowMap);
                        if (startCoordinate.x < mapRange.x || startCoordinate.y < mapRange.y ||
                            startCoordinate.x > mapRange.z || startCoordinate.y > mapRange.w)
                            Debug.Log("错误：起始超出地图范围！");

                        if (targetCoordinate.x < mapRange.x || targetCoordinate.y < mapRange.y ||
                            targetCoordinate.x > mapRange.z || targetCoordinate.y > mapRange.w)
                            Debug.Log("错误：目标超出地图范围！");
#endif
                        MapCellJobController.instance.AddPathRequest(startCoordinate, targetCoordinate, nowMap,
                            (Stack<int2> path, int map, int2 start, int2 end) =>
                        {
                            if (path.Count == 0)
                                Debug.Log(
                                    $"PlayerMove：startCoordinate{start}targetCoordinate{end}-nowMap{map}");

                            roadCells.Add(map, path);
                            roomCount--;
                            if (roomCount == 0)
                            {
                                Move(true);
                            }
                        });
                    }

                    nowMap = nextMap;
                    startCoordinate = targetMapCell;
                }

                return true;

            }
            else
            {
                FailedMoveAction();
                return false;
            }

            void Move(bool zero)
            {
                if (roomQueue.Count > 0)
                {
                    int map = roomQueue.Dequeue();
                    if(roadCells.TryGetValue(map,out var path))
                    {
                        if (path.Count == 0)
                        {
                            Debug.Log($"{characterData.characterName}map{map}寻路失败:path.Count == 0");
                            FailedMoveAction();
                            return;
                        }
                        if (!zero)
                        {
                            var coordinate = path.Pop();
                            SetCoordinate(new int3(coordinate.xy, map));
                        }
                        PlayerMove(path, () =>
                        {
                            //Debug.Log($"character:{name}--PlayerMovePathEnd");
                            if (this == CharacterManager.instance.controllerCharacter)
                            {
                                CompleteMoveAfterMapLerp();
                            }
                            else
                            {
                                Move(false);
                            }
                        }, ChangeCoordinateAction, FailedMoveAction);
                    }

                }
                else
                {
                    CompleteMove();
                }
            }
        }

    }


    public void PlayerMove(Stack<int2> pathNodes, MoveEndAction endAction = null, MoveEndAction changeCoordinateAction = null,
        MoveEndAction failedMoveAction = null)
    {
        canMove = true;
        if (pathNodes.Count > 0)
        {
            CharacterManager.instance.CharacterMoveTarget(this, pathNodes, endAction, changeCoordinateAction, failedMoveAction);
        }
        else
        {

            endAction?.Invoke();
            moveState.Clear();
        }
    }
}