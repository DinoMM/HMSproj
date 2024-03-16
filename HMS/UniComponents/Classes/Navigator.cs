using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public class Navigator
    {
        private Stack<string> urlList;
        private NavigationManager _NavigationManager;
        bool init = false;

        public string ActualPageUrl //vrati aktualnu cestu na ktorej ma byt navigator
        {
            get
            {
                string str = "";
                foreach (var item in urlList)
                {
                    str = item + str;
                }
                if (urlList.Count == 0) //ak neni nic v zasobniku tak sa vrati zbase url
                {
                    return "/";
                }
                return str;
            }
            private set { }
        }

        public Navigator(NavigationManager NavigationManager)
        {
            urlList = new();
        }

        public void InitializeNavManazer(NavigationManager NavigationManager)
        {
            if (!init)
            {
                _NavigationManager = NavigationManager;
                init = true;
            }
        }

        public string AddNextUrl(string url) //prida dalsi navazny url link, vracia aktualnu cestu po pridanom url - format "/url" alebo "url"
        {
            if (url.ElementAt(0) != '/')
            {
                url = "/" + url;
            }
            urlList.Push(url);
            return ActualPageUrl;
        }

        public string RemoveLastUrl()
        {
            if (urlList.Count != 0)
            {
                urlList.Pop();
            }
            return ActualPageUrl;
        }
        public string SwitchLast(string url) //odstrani posledne url za vlozi na koniec nove
        {
            RemoveLastUrl();
            return AddNextUrl(url);
        }

        public string SetUrl(string wholeUrl, bool cut = true) //vymaze aktualnu url a posklada z vlozeneho
        {
            urlList.Clear();
            if (cut)
            {
                var cutted = wholeUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (cutted.Length == 0)
                {
                    return ActualPageUrl;
                }
                foreach (var item in cutted)
                {
                    urlList.Push("/" + item);
                }
                return ActualPageUrl;
            }
            else
            {
                return AddNextUrl(wholeUrl);
            }

        }

        public int Count()  //pocet vlozenych url
        {
            return urlList.Count;
        }
        /// <summary>
        /// Pouzitie klasickeho NavigatorManagera
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="forceLoad"></param>
        /// <param name="replace"></param>
        /// <param name="saveThisUrl">Ulozi tuto url v Navigatore</param>
        public void NavigateTo([StringSyntax(StringSyntaxAttribute.Uri)] string uri, bool forceLoad = false, bool replace = false, bool saveThisUrl = false)
        {
            if (saveThisUrl)
            {
                SetUrl(uri);
            }
            _NavigationManager.NavigateTo(uri);
        }




    }

}
