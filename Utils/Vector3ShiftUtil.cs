using UnityEngine;

public static class Vector3ShiftUtil
{
    private static bool IsUpperLeftHalfPlane(Vector2 vector) => vector.y - vector.x > 0;
    private static bool IsUpperRightHalfPlane(Vector2 vector) => vector.y + vector.x > 0;
    private static bool IsLowerRightHalfPlane(Vector2 vector) => vector.y - vector.x < 0;
    private static bool IsLowerLeftHalfPlane(Vector2 vector) => vector.y + vector.x < 0;


    public static bool CheckIsDownShifted(Vector2 current, Vector2 compared) =>
        IsUpperLeftHalfPlane(current - compared) && IsUpperRightHalfPlane(current - compared);
        
    public static  bool CheckIsRightShifted(Vector2 current, Vector2 compared) =>
        IsUpperLeftHalfPlane(current - compared) && IsLowerLeftHalfPlane(current - compared);
        
    public static  bool CheckIsLeftShifted(Vector2 current, Vector2 compared) =>
        IsUpperRightHalfPlane(current - compared) && IsLowerRightHalfPlane(current - compared);
        
    public static bool CheckIsUpShifted(Vector2 current, Vector2 compared) =>
        IsLowerRightHalfPlane(current - compared) && IsLowerLeftHalfPlane(current - compared);
}
