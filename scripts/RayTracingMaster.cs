using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    private RenderTexture _target; // a RenderTexture is a texture that Unity creates and updates at runtime.
    private Camera _camera;
    public Texture SkyboxTexture;
    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        /* 
        Event function after the camera has finished rendering, can modify Camera's final image.
        If we want to apply some material or processing effect, we read the pixels from the <src>, apply the effect, then copy the result
        to the <dest>.
        */
        SetShaderParameters();
        Render(dest);
    }
    private void Render(RenderTexture dest)
    {
        // Create our render target if we don't have one
        InitRenderTexture();

        RayTracingShader.SetTexture(0, "Result", _target);

        // calculate the threadGroups for the GPU. In our shader, we are using the default numthreads(8,8,1), so we set one thread group to a
        // region of 8x8 pixels.
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);

        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Blit copies the _target texture onto <dest>.
        Graphics.Blit(_target, dest);
    }

    private void InitRenderTexture()
    {
        /*
        This formats the RenderTexture so that it fills up the entire view of the screen.
        */
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // release any current render texture
            if (_target != null)
            {
                _target.Release(); // releases the resources being used by the RenderTexture
            }


            // create the render texture to fill up the entire player. Use the ARGBFloat format. Linear read/write mode -> don't perform 
            // colour conversions.
            _target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }

    private void SetShaderParameters()
    {
        // pass relevant matrices into the shader
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
    }
}
