using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using UnityEngine.UI;

public class BodySourceView : MonoBehaviour 
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;
    public Image img;
    public Image img2;
    public Text debugText;
    public Text debugText2;
    public Text debugText3;

    public Text debugText4;
    public Text debugText5;
    public Text debugText6;
    public float talToMove;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft }, //
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight }, //
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };
    
    void Update () 
    {
        if (BodySourceManager == null)
        {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
                
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId) )
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body,body.TrackingId); // creat Body
                    Debug.Log("Gen New Body "+ body.TrackingId);
                }
                
                RefreshBodyObject(body, _Bodies[body.TrackingId]); // move Joint
      
            }
        }
    }
    private GameObject CreateBodyObject(Kinect.Body bodyOBJ,ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        img.enabled = false; img2.enabled = false;
        startPositonBody = true;
        firstYLeft = true; firstYRight = true;
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++) // for each bone (JointType)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);
            
            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform; 

        }
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = bodyOBJ.Joints[jt];
            Transform jointObj2 = body.transform.Find(jt.ToString()); // get obj joint form jt
            jointObj2.localPosition = GetVector3FromJoint(sourceJoint);
            if (startPositonBody)
            {
                if (jt.Equals(Kinect.JointType.AnkleLeft))
                {
                    start_AnkleLeft_z = jointObj2.localPosition.z;
                }
                if (jt.Equals(Kinect.JointType.AnkleRight))
                {
                    start_AnkleRight_z = jointObj2.localPosition.z;
                }
            }
        }

        return body;
    }
    bool startPositonBody = true,firstYLeft = true, firstYRight = true,startPointLeft = true,startPointRight=true; 
    static public bool tuchRight = false, tuchLeft = false;
    float start_AnkleLeft_z, start_AnkleLeft_y;
    float start_AnkleRight_z, start_AnkleRight_y;
    float max_AnkleLeft_y = 0, max_AnkleRight_y = 0;
    


    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null; 

            if (_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]]; // select bone to move
            }
            
            Transform jointObj = bodyObject.transform.Find(jt.ToString()); // get obj joint form jt
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);

            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {

                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));

                // Left
                if (jt.Equals(Kinect.JointType.AnkleLeft))
                {
                    if(firstYLeft)
                    {
                        start_AnkleLeft_y = GetVector3FromJoint(targetJoint.Value).y;
                        max_AnkleLeft_y = start_AnkleLeft_y;
                        firstYLeft = false;
                    }
                    float valueDifZLeft = start_AnkleLeft_z - GetVector3FromJoint(targetJoint.Value).z;
                    float valueDifYLeft = start_AnkleLeft_y - GetVector3FromJoint(targetJoint.Value).y;

                    debugText.text = start_AnkleLeft_y.ToString();
                        debugText2.text = max_AnkleLeft_y.ToString(); // 
                        debugText3.text = valueDifYLeft.ToString(); //

                        debugText4.text = start_AnkleLeft_z.ToString();
                        debugText5.text = jointObj.localPosition.z.ToString();
                        debugText6.text = (start_AnkleLeft_z - GetVector3FromJoint(targetJoint.Value).z).ToString();

                    if (valueDifZLeft < 1.5)
                    {
                        startPointLeft = true;
                    }

                    if (max_AnkleLeft_y < GetVector3FromJoint(targetJoint.Value).y && startPointLeft)
                    {
                        max_AnkleLeft_y = GetVector3FromJoint(targetJoint.Value).y;
                    }
                    if (max_AnkleLeft_y - start_AnkleLeft_y > talToMove)
                    {
                        if (valueDifZLeft > 2.5 && valueDifYLeft > -0.5)
                        {
                            img.enabled = true;
                            startPointLeft = false;
                            tuchLeft = true; 
                        }
                        else if(tuchLeft)
                        {
                            img.enabled = false;
                            tuchLeft = false;
                            max_AnkleLeft_y = start_AnkleLeft_y;
                        }
                        
                    }
                    
                }
                // Right
                if (jt.Equals(Kinect.JointType.AnkleRight))
                {
                    if (firstYRight)
                    {
                        start_AnkleRight_y = GetVector3FromJoint(targetJoint.Value).y;
                        max_AnkleRight_y = start_AnkleRight_y;
                        firstYRight = false;
                    }


                    float valuDifzRight = start_AnkleRight_z - GetVector3FromJoint(targetJoint.Value).z;
                    if (valuDifzRight < 1.5)
                    {
                        startPointRight = true;
                    }

                    if (max_AnkleRight_y < GetVector3FromJoint(targetJoint.Value).y && startPointRight)
                    {
                        max_AnkleRight_y = GetVector3FromJoint(targetJoint.Value).y;
                    }
                    if (max_AnkleRight_y - start_AnkleRight_y > talToMove)
                    {
                        if (valuDifzRight > 2.5)
                        {
                            img2.enabled = true;
                            startPointRight = false;
                            tuchRight = true;
                        }
                        else if (tuchRight)
                        {
                            img2.enabled = false;
                            tuchRight = false;
                            max_AnkleRight_y = start_AnkleRight_y;
                        }

                    }

                }

            }
            else
            {
                lr.enabled = false;
            }
        }
        startPositonBody = false;
    }
    
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
