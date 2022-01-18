using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeTest : MonoBehaviour
{
    public ComputeShader _shader;
    public RenderTexture _texture;
    void Start()
    {
        //Creation d'un renderTexture
        _texture = new RenderTexture(256,256,24);
        _texture.enableRandomWrite = true;
        _texture.Create();

        //Passe l'info au shader : [id] s'il y a plusieurs instance de ce shader , le nom de la texture dans le shader, reference de la texture unity
        _shader.SetTexture(0, "Result" , _texture);

        //[32x32x1] (1024) Groupe de -> [8x8x1] (64) threads
        // on retoruve bien 1024*65 = 65635 threads
        //la résolution de la terxture était de 256*256 = 65635
        _shader.Dispatch(0, _texture.width/8, _texture.height/8, 1);
    
    }

}
