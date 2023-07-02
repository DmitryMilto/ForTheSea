using DG.Tweening;
using UnityEngine;

public static class ProjectCustomTools
{
    public static float Remap(float In, Vector2 InMinMax, Vector2 OutMinMax) => OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);

    public static void Rotate2DImageTowards(Transform image, Vector3 targetPosition, float rotationDuration)
    {
        var direction = (targetPosition - image.position).normalized;
                    
        // Where image should face
        //var FaceDirection = targetPosition.x > Screen.width / 2 ? Vector3.right : Vector3.left;
        var FaceDirection = targetPosition.x > image.position.x ? Vector3.right : Vector3.left;
        var right = FaceDirection == Vector3.right ? direction : Vector3.Scale(direction, Vector3.one * -1);

        // Rotate on Z axis
        DOTween.To(() => image.right, value => image.right = value, right, rotationDuration);

        // Flip on X axis
        image.DOScaleX(FaceDirection == Vector3.right ? -1 : 1, rotationDuration);
    }
}
