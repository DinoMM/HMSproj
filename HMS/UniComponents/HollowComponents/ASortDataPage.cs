using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    /// <summary>
    /// Pridáva základne metódy pre triedenie zoznamu dát.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ASortDataPage<T> : ComponentBase
    {
        /// <summary>
        /// Treba inicializovať (prelinkovať) s originálnym zoznamom, ktorý sa bude triediť
        /// </summary>
        public List<T>? ZoznamSort;

        /// <summary>
        /// Treba inicializovať (@ref) s modalom SelectModal, ktorý sa použije na výber hodnôt pre triedenie v prípade, že máme stĺpce so selektívnimi hodnotami
        /// </summary>
        protected SelectModal SortSelectModal;

        protected CompLoader? SortSelCompLoader;

        #region SortData process
        /// <summary>
        /// Vyhľadá v zozname ZoznamSort položku s názvom propertyName -> na základe nej sa utriedi zoznam podľa sortParameter
        /// </summary>
        /// <param name="propertyName">String názov property triedy</param>
        /// <param name="sortParameter">Podporované typy: bool, List<string></param>
        /// <returns>Utiedi zoznam: bool - ASC/DESC, List<string> - podľa zvolenej selekcie z modalu </returns>
        public async Task Sort(string propertyName, object sortParameter)
        {
            if (sortParameter is bool sortDirection)
            {
                await SortData(propertyName, sortDirection);
            }
            else if (sortParameter is List<string> selectionList)
            {
                await SortData(propertyName, selectionList);
            }
        }

        public async Task<List<T>> SortList(string propertyName, object sortParameter, List<T> list, Func<T, object?>? cellConvert = null)
        {
            if (sortParameter is bool sortDirection)
            {
                if (cellConvert != null)
                {
                    return await SortDataList(propertyName, sortDirection, list, cellConvert);
                }
                else
                {
                    return await SortDataList(propertyName, sortDirection, list);
                }
            }
            else if (sortParameter is List<string> selectionList)
            {
                //if (cellConvert == null)
                //{
                    return await SortDataList(propertyName, selectionList, list);
                //}
                //else
                //{
                //    return await SortDataList(propertyName, selectionList, list, cellConvert);
                //}
            }
            return new List<T>();
        }
        #endregion
        #region SortData solutions
        /// <summary>
        /// Triedenie podla ASC/DESC podľa sortDirection - true = ASC, false = DESC
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        protected async Task SortData(string propertyName, bool sortDirection)
        {
            if (ZoznamSort == null)
            {
                throw new NullReferenceException(message: "Zabudol si inicializovať ZoznamSort v OnInitializedAsync");
            }

            var propertyInfo = typeof(T).GetProperty(propertyName);
            if (propertyInfo != null)
            {
                await Task.Run(() =>
                {
                    if (sortDirection) //ascending
                    {
                        ZoznamSort.Sort((x, y) =>
                        {
                            var valueX = propertyInfo.GetValue(x);
                            var valueY = propertyInfo.GetValue(y);
                            return Comparer<object>.Default.Compare(valueX, valueY);
                        });
                    }
                    else //descending
                    {
                        ZoznamSort.Sort((x, y) =>
                        {
                            var valueX = propertyInfo.GetValue(x);
                            var valueY = propertyInfo.GetValue(y);
                            return Comparer<object>.Default.Compare(valueY, valueX);
                        });
                    }
                });
                StateHasChanged();
            }
        }

        /// <summary>
        /// Triedenie podľa zovlenej hodnoty v SelectModalu
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="zoznamSelect"></param>
        /// <returns></returns>
        protected async Task SortData(string propertyName, List<string> zoznamSelect)
        {
            SortSelectModal.ID = propertyName + "selectmodal";
            SortSelectModal.SelectionList = zoznamSelect;
            if (await SortSelectModal.OpenModal(true))
            {
                var selectedValue = SortSelectModal.Value;
                if (string.IsNullOrEmpty(selectedValue))
                {
                    return;
                }
                var propertyInfo = typeof(T).GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    await Task.Run(() =>
                    {
                        ZoznamSort.Sort((x, y) =>
                        {
                            var valueX = propertyInfo.GetValue(x)?.ToString();
                            var valueY = propertyInfo.GetValue(y)?.ToString();
                            if (valueX == selectedValue && valueY != selectedValue)
                            {
                                return -1; // valueX should come before valueY
                            }
                            else if (valueX != selectedValue && valueY == selectedValue)
                            {
                                return 1; // valueY should come before valueX
                            }
                            else
                            {
                                return 0; // No need to sort the rest
                            }
                        });
                    });
                    StateHasChanged();
                }
            }
        }
        #endregion
        #region SortDataList solutions
        /// <summary>
        /// Triedenie podla ASC/DESC podľa sortDirection - true = ASC, false = DESC
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="sortDirection"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        protected async Task<List<T>> SortDataList(string propertyName, bool sortDirection, List<T> list)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName);
            if (propertyInfo != null)
            {
                await Task.Run(() =>
                {
                    if (sortDirection) //ascending
                    {
                        list.Sort((x, y) =>
                        {
                            var valueX = propertyInfo.GetValue(x);
                            var valueY = propertyInfo.GetValue(y);
                            return Comparer<object>.Default.Compare(valueX, valueY);
                        });
                    }
                    else //descending
                    {
                        list.Sort((x, y) =>
                        {
                            var valueX = propertyInfo.GetValue(x);
                            var valueY = propertyInfo.GetValue(y);
                            return Comparer<object>.Default.Compare(valueY, valueX);
                        });
                    }
                });
                StateHasChanged();
            }
            return list;
        }

        /// <summary>
        /// Triedenie podla ASC/DESC podľa sortDirection - true = ASC, false = DESC. cellValue označuje hodnotu podľa ktorej sa bude triediť. propertyName nie je potrebná ale pre kontrolu je implementovaná ako overenie existencie
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="sortDirection"></param>
        /// <param name="list"></param>
        /// <param name="cellValue"></param>
        /// <returns></returns>
        protected async Task<List<T>> SortDataList(string propertyName, bool sortDirection, List<T> list, Func<T, object?> cellValue)
        {
            //var propertyInfo = typeof(T).GetProperty(propertyName);
            //if (propertyInfo != null)
            //{
            await Task.Run(() =>
            {
                if (sortDirection) //ascending
                {
                    list.Sort((x, y) =>
                    {
                        var valueX = cellValue.Invoke(x);
                        var valueY = cellValue.Invoke(y);
                        return Comparer<object>.Default.Compare(valueX, valueY);
                    });
                }
                else //descending
                {
                    list.Sort((x, y) =>
                    {
                        var valueX = cellValue.Invoke(x);
                        var valueY = cellValue.Invoke(y);
                        return Comparer<object>.Default.Compare(valueY, valueX);
                    });
                }
            });
            StateHasChanged();
            return list;
            //}
        }

        /// <summary>
        /// Triedenie podľa zovlenej hodnoty v SelectModalu
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="zoznamSelect"></param>
        /// <returns></returns>
        protected async Task<List<T>> SortDataList(string propertyName, List<string> zoznamSelect, List<T> list)
        {
            SortSelectModal.SelectionList = zoznamSelect;
            if (await SortSelectModal.OpenModal(true))
            {
                var selectedValue = SortSelectModal.Value;
                if (string.IsNullOrEmpty(selectedValue))
                {
                    return list;
                }
                var propertyInfo = typeof(T).GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    await Task.Run(() =>
                    {
                        list.Sort((x, y) =>
                        {
                            var valueX = propertyInfo.GetValue(x)?.ToString();
                            var valueY = propertyInfo.GetValue(y)?.ToString();
                            if (valueX == selectedValue && valueY != selectedValue)
                            {
                                return -1; // valueX should come before valueY
                            }
                            else if (valueX != selectedValue && valueY == selectedValue)
                            {
                                return 1; // valueY should come before valueX
                            }
                            else
                            {
                                return 0; // No need to sort the rest
                            }
                        });
                    });
                    StateHasChanged();
                }
            }

            return list;
        }

        /// <summary>
        /// Triedenie podľa zovlenej hodnoty v SelectModalu pomocou cellConvert
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="zoznamSelect"></param>
        /// <returns></returns>
        protected async Task<List<T>> SortDataList(string propertyName, List<string> zoznamSelect, List<T> list, Func<T, object?> cellValue)
        {
            SortSelectModal.SelectionList = zoznamSelect;
            if (await SortSelectModal.OpenModal(true))
            {
                var selectedValue = SortSelectModal.Value;
                if (string.IsNullOrEmpty(selectedValue))
                {
                    return list;
                }
                //var propertyInfo = typeof(T).GetProperty(propertyName);
                //if (propertyInfo != null)
                //{
                    await Task.Run(() =>
                    {
                        list.Sort((x, y) =>
                        {
                            var valueX = cellValue.Invoke(x);
                            var valueY = cellValue.Invoke(y);
                            if (valueX == selectedValue && valueY != selectedValue)
                            {
                                return -1; // valueX should come before valueY
                            }
                            else if (valueX != selectedValue && valueY == selectedValue)
                            {
                                return 1; // valueY should come before valueX
                            }
                            else
                            {
                                return 0; // No need to sort the rest
                            }
                        });
                    });
                    StateHasChanged();
                //}
            }

            return list;
        }
        #endregion
    }
}
