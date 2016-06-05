// EXAMPLE WITH CAMERA UPSIDEDOWN
function OnPreCull() {
    GetComponent.<UnityEngine.Camera>().projectionMatrix = GetComponent.<UnityEngine.Camera>().projectionMatrix * Matrix4x4.Scale(Vector3(-1, 1, 1));
}

function OnPreRender() {
    GL.SetRevertBackfacing(true);
}

function OnPostRender() {
    GL.SetRevertBackfacing(false);
}
