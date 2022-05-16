using System;
using UnityEngine;

namespace neeksdk.Scripts.FigureTracer
{
    public class SortingOrderHelper : MonoBehaviour
    {
        [SerializeField] private SortingOrderData[] _renderers;

        private const int SORTING_ORDER_MULTIPLIER = 20;
        
        public void SetSortingOrder(int order) => AddSortingOrder(order + SORTING_ORDER_MULTIPLIER);
        public void ResetSortingOrder() => AddSortingOrder(0);

        private void AddSortingOrder(int sortingOrder)
        {
            foreach (SortingOrderData orderData in _renderers)
            {
                orderData.ObjectRenderer.sortingOrder = sortingOrder + orderData.InitialSortingOrder;
            }
        }

        [Serializable]
        private class SortingOrderData
        {
            [SerializeField] public Renderer ObjectRenderer;
            [SerializeField] public int InitialSortingOrder;
        }
    }
}