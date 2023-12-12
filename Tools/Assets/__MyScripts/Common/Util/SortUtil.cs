using System.Collections;
using System.Collections.Generic;



namespace Z.Util.Sort
{
    public static class SortUtil 
    {

        public static void Sort(int[] arr)
        {
            if (arr == null || arr.Length == 0)
                return;

            SortUP(arr, 0, arr.Length - 1);
        }

        public static void SortUp(int[] arr)
        {
            if (arr == null || arr.Length == 0)
                return;

            SortUP(arr, 0, arr.Length - 1);
        }

        public static void SortUP(int[] arr, int left, int right)
        {
            if (left >= right)
                return;

            int pivot = arr[(left + right) / 2];
            int i = left, j = right;

            while (i <= j)
            {
                while (arr[i] < pivot)
                    i++;
                while (arr[j] > pivot)
                    j--;

                if (i <= j)
                {
                    int temp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = temp;
                    i++;
                    j--;
                }
            }

            if (left < j)
                SortUP(arr, left, j);
            if (i < right)
                SortUP(arr, i, right);
        }

        public static void SortDown(int[] arr)
        {
            if (arr == null || arr.Length == 0)
                return;

            SortDown(arr, 0, arr.Length - 1);
        }

        public static void SortDown(int[] arr, int left, int right)
        {
            if (left >= right)
                return;

            int pivot = arr[(left + right) / 2];
            int i = left, j = right;

            while (i <= j)
            {
                while (arr[i] > pivot)
                    i++;
                while (arr[j] < pivot)
                    j--;

                if (i <= j)
                {
                    int temp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = temp;
                    i++;
                    j--;
                }
            }

            if (left < j)
                SortDown(arr, left, j);
            if (i < right)
                SortDown(arr, i, right);
        }
    }
}
