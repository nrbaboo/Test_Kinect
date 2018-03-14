using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbolMove : MonoBehaviour {

    float x,speed;
    // Use this for initialization
    void Start () {
     
    }
	
	// Update is called once per frame
	void Update () {
        if (x >= SymbolMode1.xEnd)
        {
            SymbolMode1.creatSymbol();
            Destroy(gameObject);
        }
        transform.Translate(Vector3.right * Time.deltaTime * SymbolMode1.speedSym);
        x = transform.position.x;
       if(BodySourceView.tuchLeft)
        {
            SymbolMode1.creatSymbol();
            Destroy(gameObject);
        }
    }
    
}
