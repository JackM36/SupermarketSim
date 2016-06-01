using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProductsManager))]
public class ProductsManagerEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        /*
        int productCategoriesNumber = System.Enum.GetNames(typeof(ProductsManager.ProductCategory)).Length;

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Product Categories (" + productCategoriesNumber + ")", EditorStyles.boldLabel);
        GUILayout.EndHorizontal();

        for (int i = 0; i < productCategoriesNumber; i++)
        {
            GUILayout.BeginHorizontal();
                string categoryName = ((ProductsManager.ProductCategory)i).ToString();
                EditorGUILayout.LabelField(categoryName);
            GUILayout.EndHorizontal();
        }
        */
        DrawDefaultInspector();
    }
}
