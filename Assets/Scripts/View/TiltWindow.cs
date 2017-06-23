using UnityEngine;

public class TiltWindow : MonoBehaviour
{
    //我姑且猜想这是一个摆动的范围
	public Vector2 range = new Vector2(5f, 3f);

	Transform mTrans;
	Quaternion mStart;
	Vector2 mRot = Vector2.zero;

	void Start ()
	{
		mTrans = transform;
		mStart = mTrans.localRotation;
	}

	void Update ()
	{
		Vector3 pos = Input.mousePosition;

		float halfWidth = Screen.width * 0.5f;
		float halfHeight = Screen.height * 0.5f;
        //以屏幕中心为基准
        //计算x、y轴上的比例
        //从而对窗口进行不同比例的旋转偏移
		float x = Mathf.Clamp((pos.x - halfWidth) / halfWidth, -1f, 1f);
		float y = Mathf.Clamp((pos.y - halfHeight) / halfHeight, -1f, 1f);
        //对鼠标移动前后的位置插值从而实现渐变旋转的效果
		mRot = Vector2.Lerp(mRot, new Vector2(x, y), Time.deltaTime * 5f);

        //对x、y轴指定偏转的度数，转成四元数进行右乘旋转
		mTrans.localRotation = mStart * Quaternion.Euler(-mRot.y * range.y, mRot.x * range.x, 0f);
	}
}
