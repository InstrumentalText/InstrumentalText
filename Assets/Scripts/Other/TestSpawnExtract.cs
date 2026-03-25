using UnityEngine;

public class TestSpawnExtract : MonoBehaviour
{
    public PdfViewerHandler handler;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[Test] Trigger Extract");

            handler.Execute("pdf.extract", "{}", new ExecutionContext(handler.gameObject));
        }
    }
}