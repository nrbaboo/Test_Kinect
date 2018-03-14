using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbolMode1 : MonoBehaviour {

    public GameObject startObject;
    public GameObject endObject;
    public GameObject symbolLeft;

    public float speed = 2f;
    public static float xStart,xEnd,speedSym;
    static public GameObject staticSymbolLeft;
    // Use this for initialization
    void Start () {
        speedSym = speed;
        xStart = startObject.transform.position.x;
        xEnd = endObject.transform.position.x;
        staticSymbolLeft = symbolLeft;
    }
	
	// Update is called once per frame
	void Update () {


    }
    
    static public void creatSymbol()
    {
        Instantiate(staticSymbolLeft, staticSymbolLeft.transform.position, Quaternion.identity);
    }
}
