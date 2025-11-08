using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] float riseSpeed = 1.2f;
    [SerializeField] float remainTime = 0.7f;

    float t;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Setup(int damage, Color? color = null)
    {
        text.text = damage.ToString();
        var c = color ?? Color.white;
        c.a = 1f;
        text.color = c;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //floating font
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        //Always look at Camera
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        //Becomes transparent
        t += Time.deltaTime;
        var c = text.color;
        c.a = Mathf.Lerp(1f, 0f, t / remainTime);
        text.color = c;

        if (t >= remainTime) {
            Debug.Log("destroyed");
            Destroy(gameObject);
        }
    }
}
