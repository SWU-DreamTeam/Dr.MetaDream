using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;


public class LPEP_Shuffler : MonoBehaviour
{

    // the counts will decide which choice is made.

    private int hatCount = 0; // count
   // private int hatID = 0; // choice
    private int glassesCount = 0;
   // private int glassesID = 0;
   // private int HeadsCount = 0;
   // private int HeadsID = 0;
    
    private int HairsCount = 0;
    private int HairsID = 0;

    private int beardsCount = 0;
    private int beardsID = 0;
    private Color newSkinTone;
    private Color newHairTone;
    private Color color1 = new Color (0.82f, 0.7f, 0.64f, 1.0f);
    private Color color2 = new Color(0.18f, 0.14f, 0.08f, 1.0f);
    private Color hair1 = new Color(0.8f, 0.1f, 0.0f, 1.0f);
    private Color hair2 = new Color(1.0f, 0.75f, 0.5f, 1.0f);
    public bool isFemale = false;
    private bool hasGlasses = false;
    private float glassesDice = 0.0f;
    private float hairDarken = 1.0f; // a multiplier to darken 0.0 will darken
    // public switches
    
  

    public List<GameObject> Heads;
    public List<GameObject> Hats;
    public List<GameObject> Glasses;
    public List<GameObject> Hairs;
    public List<GameObject> Beards;
    //public List<GameObject> Bodys;

    private Renderer oRenderer;

    // Use this for initialization
    void Start()
    {
        //initialize
        hatCount = 0;
        

                // populate list based on tags
        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {

            if (child.tag == "Head")
            {
                Heads.Add(child.gameObject);
                //Debug.Log(child + " added");

                //maleHeadsCount++; // increase
            }

            if (child.tag == "Hats")
            {
                Hats.Add(child.gameObject);
                //Debug.Log(child + " added");

                hatCount++; // increase
            }

            if (child.tag == "Glasses")
            {
                Glasses.Add(child.gameObject);
                //Debug.Log(child + " added");
                glassesCount++;
            }

            if (child.tag == "Hair")
            {
                Hairs.Add(child.gameObject);
                //Debug.Log(child + " added");
                HairsCount++;
            }


            if (child.tag == "Beards")
            {
                Beards.Add(child.gameObject);
                //Debug.Log(child + " added");
                beardsCount++;
            }

        }

        // INITIAL PICKING.
        pickHat();
        pickHair();
        pickBeard();
        decideGlasses();
        makeSkinTone();
    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            // INPUT BASED PICKING (SPACE)
            pickHat();
            pickHair();
            pickBeard();
            decideGlasses();
            makeSkinTone();
        }
    }


    void makeSkinTone()
    {
        newSkinTone = Color.Lerp(color1, color2, Random.Range(0.0f, 1.0f));
        
        // populate list based on tags
        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            // change body color for head and body
            if ((child.tag == "Body") || (child.tag == "Head"))
            {
                //Bodys.Add(child.gameObject);
                //Debug.Log(child + " added");
                oRenderer = child.GetComponentInChildren<Renderer>();
                oRenderer.material.SetColor("_MainSkinColor", newSkinTone);

            }

        }
    }

    void decideGlasses()
    {
        glassesDice = (Random.Range(0.0f, 1.0f));

        if (glassesDice > 0.5f) {hasGlasses = true; } else { hasGlasses = false; }


    if (!hasGlasses)
        {
            foreach (Transform child in transform.GetComponentsInChildren<Transform>())
            {
                if (child.tag == "Glasses")
                {
                    oRenderer = child.GetComponentInChildren<Renderer>();
                    oRenderer.enabled = false;
                }
            }
        }
        else
        {
            foreach (Transform child in transform.GetComponentsInChildren<Transform>())
            {
                if (child.tag == "Glasses")
                {
                    oRenderer = child.GetComponentInChildren<Renderer>();
                    oRenderer.enabled = true;
                }
            }
        }
    }

    void pickBeard()
    {
        if (!isFemale) // only men have beards (unless you are the bearded lady)
        {
             
            
            beardsCount = 0;
            beardsID = Random.Range(-1, Beards.Count); // -1 is bald
            foreach (GameObject o in Beards)
            {
                if (beardsCount == beardsID)
                {
                    oRenderer = o.GetComponentInChildren<Renderer>();
                    oRenderer.enabled = true;
                    oRenderer.material.SetColor("_FacialHair_1stColor", newHairTone);
                    oRenderer.material.SetColor("_FacialHair_2ndColor", newHairTone);
                }
                else
                {
                    oRenderer = o.GetComponentInChildren<Renderer>();
                    oRenderer.enabled = false;
                }


                beardsCount++;
            }
        }
    }

    // Function for picking hats
    void pickHat()
    {
        // go through list of hats, choose 1
        // then edit the shader vertex color
        // for head and hair

    }

    void pickHair()
    {
        hairDarken = Random.Range(0.0f, 1.0f);
        hair1 = new Color(0.8f* hairDarken, 0.1f* hairDarken, 0.0f* hairDarken, 1.0f);
        hair2 = new Color(1.0f* hairDarken, 1.0f*hairDarken, 0.5f* hairDarken, 1.0f);
        newHairTone = Color.Lerp(hair1, hair2, Random.Range(0.0f, 1.0f)); // also randomise the hair tones

        
        HairsCount = 0;
        if (!isFemale) { HairsID = Random.Range(-1, Hairs.Count); } // -1 is bald
        if (isFemale)
            {
                HairsID = Random.Range(0, Hairs.Count);
            }


            
           

            foreach (GameObject o in Hairs)
            {
                if (HairsCount == HairsID)
                {
                    oRenderer = o.GetComponentInChildren<Renderer>();
                    oRenderer.enabled = true;
                    oRenderer.material.SetColor("_Hair_1stColor", newHairTone);
                    oRenderer.material.SetColor("_Hair_2ndColor", newHairTone);
                }
                else
                {
                    oRenderer = o.GetComponentInChildren<Renderer>();
                    oRenderer.enabled = false;
                }


                HairsCount++;
            }

        
        // check head for scalp requirement
        // bear in mind the material will be instanced
        // so wont match the hands / neck
        foreach (GameObject o in Heads)
        {
            if (HairsID == -1)  // no hair
            {
                oRenderer = o.GetComponentInChildren<Renderer>();
                oRenderer.material.SetFloat("_HeadScalp", 0.0f);
            }
            else
            {
                oRenderer = o.GetComponentInChildren<Renderer>();
                oRenderer.material.SetFloat("_HeadScalp", 1.0f);
            }
        }
        

    }

}






















