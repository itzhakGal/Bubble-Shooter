//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Bomb : MonoBehaviour
//{
//    public float explosionRadius = 5.0f;  // הגדלת רדיוס הפיצוץ של הפצצה
//    public GameObject explosionEffect;    // אפקט ויזואלי של פיצוץ

//    // מתבצע כאשר יש התנגשות עם אובייקט אחר
//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        // נוודא שהתנגשות מתבצעת רק אם פגענו ישירות בבועה
//        if (other.CompareTag("Bubble"))
//        {
//            Debug.Log("explode");
//            Explode();  // פיצוץ הפצצה
//        }
//    }

//    void Explode()
//    {
//        // יצירת אפקט הפיצוץ
//        GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
//        explosion.transform.localScale = new Vector3(25f, 25f, 1f);

//        // השמדת אפקט הפיצוץ אחרי 0.5 שניות
//        Destroy(explosion, 0.5f);

//        // חיפוש כל הבועות שנמצאות בתוך רדיוס הפיצוץ
//        Collider2D[] hitBubbles = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
//        Debug.Log("Found " + hitBubbles.Length + " bubbles in explosion radius.");

//        foreach (var hit in hitBubbles)
//        {
//            // בדיקה אם האובייקט הוא בועה
//            if (hit.CompareTag("Bubble"))
//            {
//                Debug.Log("Destroying bubble: " + hit.gameObject.name);
//                // השמדת כל הבועות שנמצאות ברדיוס הפיצוץ
//                Destroy(hit.gameObject);
//            }
//        }

//        // השמדת הפצצה עצמה לאחר הפיצוץ
//        Destroy(gameObject);  // הפצצה מתפוצצת בעצמה
//    }

//    // ציור רדיוס הפיצוץ בעורך
//    void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(transform.position, explosionRadius);
//    }
//}
