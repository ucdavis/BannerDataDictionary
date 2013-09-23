﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BannerDataDictionary.Models;
using Dapper;

namespace BannerDataDictionary.Controllers
{
    public class SearchController : ApplicationController
    {
        //
        // GET: /Search/

        public ActionResult Index()
        {
            var viewModel = new SearchModel();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString))
            {
                conn.Open();
                viewModel.LinkedServers = conn.Query<LinkedServer>(@"EXEC usp_GetOracleLinkedServerNames").ToList();

                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Details(SearchModel model)
        {
            if (ModelState.IsValid)
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString))
                {
                    var searchString = model.SearchString;
                    var selectedServerNames = model.SelectedServerNames;
                        //This what's populated when a user selects (a) linked server(s).
                    var selectedServerNamesString = "";
                    if (selectedServerNames != null && selectedServerNames.Any())
                    {
                        selectedServerNamesString = selectedServerNames.Aggregate(selectedServerNamesString,
                                                                                  (current, name) =>
                                                                                  current + (name + ","));
                        selectedServerNamesString = selectedServerNamesString.Substring(0,
                                                                                        selectedServerNamesString.Length -
                                                                                        1);
                    }

                    conn.Open();
                    var results =
                        conn.Query<SearchResult>(
                            @"SELECT * FROM dbo.udf_GetTableColumnCommentsResults(@searchString, @LinkedServerNames)",
                            new {@searchString = searchString, @LinkedServerNames = selectedServerNamesString}).ToList();
                    return View(results);
                }
            }
            return View(model);
        }
    }
}
