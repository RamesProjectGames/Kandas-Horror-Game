using UnityEngine;
using UnityEngine.UI;

public class BetterGridLayout : LayoutGroup
{
    public enum FitPriority
    {
        Both,
        Columns,
        Rows
    }
    public enum StartAxis
    {
        Horizontal,
        Vertical
    }
    public int columns=1, rows=1;
    public FitPriority fitPriority;
    public StartAxis startAxis;
    public Vector2 cellSize, paddingSize;
    public bool setValuesManually, clampX, clampY;

    public override void CalculateLayoutInputVertical()
    {
        if((fitPriority == FitPriority.Both) || (rows == 0 && fitPriority == FitPriority.Rows) || (columns == 0 && fitPriority == FitPriority.Columns))
        {
            float sqrRt = Mathf.Sqrt((float)rectChildren.Count);
            if(!setValuesManually)
            {
                rows = Mathf.Clamp(Mathf.CeilToInt(sqrRt), 1, int.MaxValue);
                columns = Mathf.Clamp(Mathf.CeilToInt(sqrRt), 1, int.MaxValue);
            }
            clampX = true;
            clampY = true;
        }
        else if(fitPriority == FitPriority.Columns)
        {
            if(!setValuesManually)
                rows = Mathf.Clamp(Mathf.CeilToInt((float)rectChildren.Count / (float)columns), 1, int.MaxValue);
            clampX = true;
            startAxis = StartAxis.Horizontal;
            if (rectChildren.Count > 0 && !clampY)
            {
                float calcHeight = Mathf.CeilToInt(rectChildren[0].rect.height * rows + (rows - 1) * paddingSize.y + padding.top + padding.bottom);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, calcHeight);
            }
        }
        else
        {
            if(!setValuesManually)
                columns = Mathf.Clamp(Mathf.CeilToInt((float)rectChildren.Count / (float)rows), 1, int.MaxValue);
            clampY = true;
            startAxis = StartAxis.Vertical;
            if (rectChildren.Count > 0 && !clampX)
            {
                float calcWidth = Mathf.CeilToInt(rectChildren[0].rect.width * columns + (columns - 1) * paddingSize.x + padding.left + padding.right);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, calcWidth);
            }
        }
        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float cellWidth = (parentWidth - (columns - 1) * paddingSize.x - padding.left - padding.right) / columns;
        float cellHeight = (parentHeight - (rows - 1) * paddingSize.y - padding.top - padding.bottom) / rows;


        int rowCount = 0, colCount = 0;
        for (int i = 0; i < rectChildren.Count; i++)
        {
            rowCount = startAxis == StartAxis.Horizontal ? i / columns : i % rows;
            colCount = startAxis == StartAxis.Horizontal ? i % columns : i / rows;

            RectTransform temp = rectChildren[i];
            cellSize = new Vector2(cellWidth, cellHeight);

            float xPos = (cellSize.x * colCount) + (paddingSize.x * colCount) + padding.left;
            float yPos = (cellSize.y * rowCount) + (paddingSize.y * rowCount) + padding.top;

            SetChildAlongAxis(temp, 0, xPos, cellSize.x);
            SetChildAlongAxis(temp, 1, yPos, cellSize.y);
        }
    }

    public override void SetLayoutHorizontal()
    {
        //base.SetLayoutHorizontal();
    }

    public override void SetLayoutVertical()
    {
        //base.SetLayoutVertical();
    }
}
