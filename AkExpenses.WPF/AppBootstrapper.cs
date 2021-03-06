﻿using AkExpenses.Client.ViewModels;
using AkExpenses.Models.Interfaces;
using AkExpenses.Models.Utitlity;
using AkExpenses.Services;
using AKSoftware.WebApi.Client;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AkExpenses.WPF
{
    public class AppBootstrapper
    {

        public AppBootstrapper()
        {
            RegisterDependencies(); 
        }

        public void RegisterDependencies()
        {
            // Register Confiugration 
            var _configuration = new Configuration("settings.json");
            _configuration.SaveValue("ApiUri", "https://localhost:44391/api");
            _configuration.SaveValue("SemosApiUri", "http://localhost:25841/api");
            Locator.CurrentMutable.RegisterConstant(_configuration, typeof(IConfiguration));
            // Register API Service 
            var _serviceClient = new ServiceClient(); 
            Locator.CurrentMutable.RegisterConstant(_serviceClient, typeof(ServiceClient));

            // Register Auth Service 
            Locator.CurrentMutable.RegisterConstant(new Auth(_serviceClient, _configuration), typeof(Auth));
        }

        public void RegisterViewForViewModels()
        {

        }
    }
}
