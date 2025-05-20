using UnityEngine;
using UnityEngine.UI;
using System.Linq;

internal class GetColor : MonoBehaviour
{
    //internal GameObject threeCirclePercentObject; // TMThreeCirclePercent를 가지고 있는 게임 오브젝트
    //internal GameObject label;
    //internal int orderNumber; // 1에서 5 사이의 숫자

    //private TMThreeCirclePercent threeCirclePercent;
    //private Image labelImage;

    //void Start()
    //{
    //    // TMThreeCirclePercent 컴포넌트를 가져옵니다.
    //    threeCirclePercent = threeCirclePercentObject.GetComponent<TMThreeCirclePercent>();

    //    // Label의 Image 컴포넌트를 가져옵니다.
    //    labelImage = label.GetComponent<Image>();

    //    // orderNumber가 1에서 5 사이인지 확인합니다.
    //    if (orderNumber < 1 || orderNumber > 5)
    //    {
    //        Debug.LogError("Order number must be between 1 and 5.");
    //    }
    //}

    //void Update()
    //{
    //    UpdateLabelColor();
    //}

    //void UpdateLabelColor()
    //{
    //    // TMThreeCirclePercent로부터 값을 가져옵니다.
    //    //var values = new float[] { threeCirclePercent.no1, threeCirclePercent.no2, threeCirclePercent.no3, threeCirclePercent.no4, threeCirclePercent.no5 };
    //    //var images = new Image[] { threeCirclePercent.a, threeCirclePercent.b, threeCirclePercent.c, threeCirclePercent.d, threeCirclePercent.e };
    //    var values = threeCirclePercent.GetValues();
    //    var images = threeCirclePercent.ringCharts.ToArray();


    //    // 값과 이미지의 쌍을 생성합니다.
    //    var valueImagePairs = values.Select((value, index) => new { Value = value, Image = images[index] }).ToList();

    //    // 값을 내림차순으로 정렬합니다.
    //    var sortedValueImagePairs = valueImagePairs.OrderByDescending(pair => pair.Value).ToList();

    //    // orderNumber에 해당하는 이미지를 선택합니다.
    //    var selectedImage = sortedValueImagePairs[orderNumber - 1].Image;
    //    var selectedColor = selectedImage.color;

    //    // 선택된 색상을 Label의 이미지 컬러값에 적용합니다.
    //    labelImage.color = selectedColor;
    //}
}
