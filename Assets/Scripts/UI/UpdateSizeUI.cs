using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UpdateSizeUI : MonoBehaviour
    {
        private TextMeshProUGUI _tmPro;

        private void Start()
        {
            _tmPro = GetComponent<TextMeshProUGUI>();
        }

        public void ChangeData(float data)
        {
            _tmPro.text = data.ToString();
        }
    }
}