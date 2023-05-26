 public ActionResult AddUpdateExpense(FormCollection collection, IEnumerable<HttpPostedFileBase> divitFile)
        {
         

            SimpleLogger logger = new SimpleLogger("AddUpdateExpense", true);
            ObjectModelLibrary.Expense _result = null;

            int relatedApplicationType=int.Parse(collection["RelatedApplicationType"].ToString());
            ObjectModelLibrary.Levy singleLevy=new ObjectModelLibrary.Levy();
            ObjectModelLibrary.Law _law = new ObjectModelLibrary.Law();

            if (relatedApplicationType == (int)Enumerations.Expense.RelatedApplicationType.Levy)
            {
                singleLevy = THOSWEB.WebApiFacade.Services.LevyService.GetSingleLevy(THOSWEB.StateManager.SessionManager.Current.CurrentUser.UserID, DataHelper.GetInt32(collection["baseappid"], 0, false), 365, null, null, null, null);
            }//Dava ve Takip Dosyası ise
            else if (relatedApplicationType == (int)Enumerations.Expense.RelatedApplicationType.Law)
            {
                ObjectModelLibrary.Law tmpLaw = new ObjectModelLibrary.Law();
                tmpLaw.LawID =  int.Parse(collection["RelatedApplicationID"].ToString());
                tmpLaw.BaseProps = new BaseProperties();
                tmpLaw.BaseProps.RequesterUserInfo = SessionManager.Current.CurrentUser;

                _law = WebApiFacade.Services.LawService.GetLaw(tmpLaw);
            }
            try
            {
                UserInfoContext _userContext = null;
                if (SessionManager.Current.CurrentUser != null)
                {
                    _userContext = SessionManager.Current.CurrentUser;
                }
                
                ObjectModelLibrary.Expense request = new ObjectModelLibrary.Expense();
                BaseProperties baseProps = new BaseProperties();
                baseProps.RequesterUserInfo = new UserInfoContext();
                request.BaseProps = baseProps;

               
                //Genel Masraf ise
                if (!Convert.ToBoolean(DataHelper.GetBool(collection["IsBankingFile"], false, false)))
                {
                    request.BaseProps.ServiceName = "FilExpenseItem";
                }
                else
                {
                    bool isBankingFile = false;

                    request.AccountNumber =DataHelper.GetInt32(collection["hdAccountNumber_"],0,false);
                    request.AccountNumberSuffix = DataHelper.GetInt32(collection["hdAccountNumberSuffix_"], 0, false);
                    request.CostType = int.Parse(collection["CostType"].ToString());

                    //Dava ve Takip Dosyası ise
                    if (relatedApplicationType == (int)Enumerations.Expense.RelatedApplicationType.Law && Convert.ToBoolean(DataHelper.GetBool(collection["IsBankingFile"], false, false)))
                    {
                        if (!string.IsNullOrEmpty(_law.CustomerNumber))
                        {
                            //ObjectModelLibrary.Levy levy = null;

                            //if (CacheManager.LevyList.List.Where(x => x.CustomerNumber == _law.CustomerNumber && ((x.IsBankingFile && x.ParentFile.LevyID == 0) || (!x.IsBankingFile && x.ParentFile.LevyID != 0))) != null)
                            //    levy = CacheManager.LevyList.List.Where(x => x.CustomerNumber == _law.CustomerNumber && ((x.IsBankingFile && x.ParentFile.LevyID == 0) || (!x.IsBankingFile && x.ParentFile.LevyID != 0))).FirstOrDefault();

                            //if (levy != null)
                            //{
                            //    var relatedLevy = THOSWEB.WebApiFacade.Services.LevyService.GetSingleLevy(THOSWEB.StateManager.SessionManager.Current.CurrentUser.UserID, DataHelper.GetInt32(levy.LevyID, 0, false), 365, null, null, null, null);
                            //    List<LevyLoan> loans = new List<LevyLoan>();
                            //    isBankingFile = relatedLevy.IsBankingFile;

                            //    //Teminattan takip başlatılan dosya ise oda Ana dosya gibi integrationFile olarak gözükecek
                            //    if (!relatedLevy.IsBankingFile && (relatedLevy.ParentFile.LevyID != 0))
                            //    {
                            //        isBankingFile = true;
                            //        loans = THOSWEB.WebApiFacade.Services.LevyService.GetLevyLoans(relatedLevy.ParentFile.LevyID).List;
                            //        var loan = loans.FirstOrDefault(x => x.LoanID == DataHelper.GetInt32(collection["loanId"], 0, false));
                            //        request.DebtNumber = DataHelper.GetInt32(loan.DebtNumber, 0, false);
                            //    }
                            //    else
                            //    {
                            //        var loan = relatedLevy.Loans.List.FirstOrDefault(x => x.LoanID == DataHelper.GetInt32(collection["loanId"], 0, false));
                            //        request.DebtNumber = DataHelper.GetInt32(loan.DebtNumber, 0, false);
                            //    }
                            //}
                            request.DebtNumber=DataHelper.GetInt32(collection["loanId"], 0, false);
                        }
                    }
                    else
                    {//İcra ve Takip Dosyası ise

                        isBankingFile = singleLevy.IsBankingFile;

                        //Teminattan takip başlatılan dosya ise oda Ana dosya gibi integrationFile olarak gözükecek
                        if (!isBankingFile && (singleLevy.ParentFile.LevyID != 0))
                        {
                            List<LevyLoan> loans = new List<LevyLoan>();
                            isBankingFile = true;
                            loans = THOSWEB.WebApiFacade.Services.LevyService.GetLevyLoans(singleLevy.ParentFile.LevyID).List;
                            var loan = loans.FirstOrDefault(x => x.LoanID == DataHelper.GetInt32(collection["loanId"], 0, false));

                            if (loan != null)
                            {
                                request.DebtNumber = DataHelper.GetInt32(loan.DebtNumber, 0, false);
                            }
                            else
                            {
                                request.DebtNumber = 0;
                            }
                            
                        }
                        else
                        {

                            var loan = singleLevy.Loans.List.FirstOrDefault(x => x.LoanID == DataHelper.GetInt32(collection["loanId"], 0, false));

                            if (loan != null)
                            {
                                request.DebtNumber = DataHelper.GetInt32(loan.DebtNumber, 0, false);
                            }
                            else
                            {
                                request.DebtNumber = 0;
                            }
                        }
                    }



                    
                    if (request.CostType == (int)THOS.Utilities.Enumerations.Enumerations.Expense.ExpenseCostType.AdvanceFundedCostType)
                        request.CostId = (int)THOS.Utilities.Enumerations.Enumerations.Expense.ExpenseCostType.AdvanceFundedCostID;
                    else
                        request.CostId = (int)THOS.Utilities.Enumerations.Enumerations.Expense.ExpenseCostType.AdvanceUnfundedCostID;
                }
                request.RelatedAdvanceId = DataHelper.GetInt32(collection["Advance"], 0, false);



                request.BaseProps.RequesterUserInfo = _userContext;
                request.ExpenseID = int.Parse(collection["relatedexpenseid"].ToString());
                //request.RelatedAdvanceId = int.Parse(collection["CaseAccountExpense"].ToString());
                request.RelatedApplicationType = relatedApplicationType;
                if (collection["RelatedApplicationID"].ToString().Trim() != "0")
                {
                    request.RelatedApplicationID = int.Parse(collection["RelatedApplicationID"].ToString());

                }


                if (int.Parse(collection["apptype"].ToString().Trim()) == 0 || int.Parse(collection["apptype"].ToString().Trim()) == 1 || int.Parse(collection["apptype"].ToString().Trim()) == 2 || int.Parse(collection["apptype"].ToString().Trim()) == 3 || int.Parse(collection["apptype"].ToString().Trim()) == 4 || int.Parse(collection["apptype"].ToString().Trim()) == 9 || int.Parse(collection["apptype"].ToString().Trim()) == 10)
                {
                    request.CurrentType = (int)THOS.Utilities.Enumerations.Enumerations.Expense.CurrentType.Expense;
                    request.CurrentTypeName = THOS.Utilities.Enumerations.EnumerationHelper.Expense.GetCurrentType(THOS.Utilities.Enumerations.Enumerations.Expense.CurrentType.Expense);
                }
                else if (int.Parse(collection["apptype"].ToString().Trim()) == 5 || int.Parse(collection["apptype"].ToString().Trim()) == 6 || int.Parse(collection["apptype"].ToString().Trim()) == 7 || int.Parse(collection["apptype"].ToString().Trim()) == 8)
                {
                    request.CurrentType = (int)THOS.Utilities.Enumerations.Enumerations.Expense.CurrentType.Income;
                    //request.IsTransfer = true;
                    //request.RelatedApplicationType = 0;
                    //request.RelatedApplicationID = 0;
                    request.CurrentTypeName = THOS.Utilities.Enumerations.EnumerationHelper.Expense.GetCurrentType(THOS.Utilities.Enumerations.Enumerations.Expense.CurrentType.Income);
                }


                request.CurrentMethod = (int)THOS.Utilities.Enumerations.Enumerations.Expense.CurrentMethod.Cash;  //DataHelper.GetInt32(int.Parse(collection["CurrentMethodExpense"].ToString()), (int)THOS.Utilities.Enumerations.Enumerations.Expense.CurrentMethod.Cash, false);
                if (request.Account == null)
                    request.Account = new CaseAccount();
                request.Account.AccountID = DataHelper.GetInt32(int.Parse(collection["CaseAccountExpense"]), 0, true);
                request.Title = collection["TitleExpense"].ToString();
                request.Description = collection["DescriptionExpense"].ToString();

                try
                {
                    DateTimeFormatInfo fi = new DateTimeFormatInfo();
                    fi.ShortDatePattern = "dd.MM.yyyy";
                    request.Date = Convert.ToDateTime(collection["expensedate"], fi);
                }
                catch (Exception dtex)
                {
                    DateTimeFormatInfo fi = new DateTimeFormatInfo();
                    fi.ShortDatePattern = "MM.dd.yyyy";
                    request.Date = Convert.ToDateTime(collection["expensedate"], fi);
                }


                if (request.Type == null)
                    request.Type = new ExpenseType();
                request.Type.ExpenseTypeID = DataHelper.GetInt32(int.Parse(collection["ExpenseTypeExpense"]), 0, true);
                request.Type.Name = DataHelper.GetString(collection["ExpenseTypeExpenseName"], string.Empty, false);
                request.DocumentedAmount = 0;
                request.UndocumentedAmount = 0;
                request.TotalAmount = DataHelper.GetDouble(collection["GeneralAmount"], 0, false);
                request.RelatedCustomer = new ObjectModelLibrary.Customer();
                request.RelatedCustomer.CustomerID = DataHelper.GetInt32(int.Parse(collection["CustomersExpense"]), 0, false);

                request.RelatedLoanId = DataHelper.GetInt32(collection["loanId"], 0, false);


                request.GeneralAmount = DataHelper.GetDouble(collection["GeneralAmount"], 0, false);
                request.IsCreditCustomer = DataHelper.GetBool(collection["IsCreditCustomer"], false, false);
                request.RelationType = DataHelper.GetInt32(collection["RelationType"], 0, false);
                if (request.GeneralAmount < 0)
                {
                    throw new Exception("Masraf için girilen değerler negatif olamaz!");
                }

                #region Gider Kalemleri
                List<ExpenseItem> expenseItemList = new List<ExpenseItem>();
                //divitlere erişmek için guid veriyoruz
                string divitGuid = Guid.NewGuid().ToString();

                if (!DataHelper.GetBool(collection["IsBankingFile"], false, false))
                {
                    //Sondaki # işareti silinerek atama işlemi yapılıyor
                    string expenseItemTypeIds = collection["ExpenseItemTypeIds"].ToString().Remove(collection["ExpenseItemTypeIds"].ToString().Length - 1);
                    string exInvoiceDates = collection["ExInvoiceDates"].ToString().Remove(collection["ExInvoiceDates"].ToString().Length - 1);
                    string companyNames = collection["CompanyNames"].ToString().Remove(collection["CompanyNames"].ToString().Length - 1);
                    string exInvoiceNos = collection["ExInvoiceNos"].ToString().Remove(collection["ExInvoiceNos"].ToString().Length - 1);
                    string taxNumbers = collection["TaxNumbers"].ToString().Remove(collection["TaxNumbers"].ToString().Length - 1);
                    string unitPrices = collection["UnitPrices"].ToString().Remove(collection["UnitPrices"].ToString().Length - 1);
                    string descriptions = collection["Descriptions"].ToString().Remove(collection["Descriptions"].ToString().Length - 1);

                    string[] expenseItemTypeIdArray = expenseItemTypeIds.Split(new string[] { "#" }, StringSplitOptions.None);
                    string[] exInvoiceDateArray = exInvoiceDates.Split(new string[] { "#" }, StringSplitOptions.None);
                    string[] companyNameArray = companyNames.Split(new string[] { "#" }, StringSplitOptions.None);
                    string[] exInvoiceNosArray = exInvoiceNos.Split(new string[] { "#" }, StringSplitOptions.None);
                    string[] taxNumbersArray = taxNumbers.Split(new string[] { "#" }, StringSplitOptions.None);
                    string[] unitPricesArray = unitPrices.Split(new string[] { "#" }, StringSplitOptions.None);
                    string[] descriptionsArray = descriptions.Split(new string[] { "#" }, StringSplitOptions.None);


                    for (int i = 0; i < expenseItemTypeIdArray.Length; i++)
                    {
                        ExpenseItem expenseItem = new ExpenseItem();
                        expenseItem.ExpenseItemTypeID = DataHelper.GetInt32(expenseItemTypeIdArray[i], 0, false);
                        expenseItem.ExInvoiceDate = (DateTime)DataHelper.GetDateTime(exInvoiceDateArray[i], DateTime.Now, false);
                        expenseItem.CompanyName = DataHelper.GetString(companyNameArray[i], string.Empty, false);
                        expenseItem.ExInvoiceNo = DataHelper.GetString(exInvoiceNosArray[i], string.Empty, false);
                        expenseItem.TaxNumber = DataHelper.GetString(taxNumbersArray[i], string.Empty, false);
                        expenseItem.UnitPrice = DataHelper.GetDouble(unitPricesArray[i], 0.0, false);
                        expenseItem.Description = DataHelper.GetString(descriptionsArray[i], string.Empty, false);
                        expenseItemList.Add(expenseItem);
                    }

                }

                request.DivitInstanceId = divitGuid;
                if (expenseItemList != null && expenseItemList.Count > 0)
                {
                    request.ExpenseItemList = expenseItemList;
                }


                //ExpenseItem expenseItem = new ExpenseItem();
                //expenseItem.ExpenseItemTypeID =  DataHelper.GetInt32(exInvoiceDates,0,false);
                //expenseItem.ExInvoiceDate = (DateTime)DataHelper.GetDateTime(expenseItemTypeIds,DateTime.Now,false);
                //expenseItem.CompanyName = DataHelper.GetString(companyNames,string.Empty,false);
                //expenseItem.ExInvoiceNo = DataHelper.GetString(exInvoiceNos,string.Empty,false);
                //expenseItem.TaxNumber = DataHelper.GetString(taxNumbers,string.Empty,false);
                //expenseItem.UnitPrice = DataHelper.GetDouble(unitPrices,0.0,false);
                //expenseItem.Description = DataHelper.GetString(descriptions,string.Empty,false);

                //THOSWEB.WebApiFacade.Services.ExpenseService.InsertExpenseItem(expenseItem);
                #endregion


                //ObjectModelLibrary.Expense _result = WebApiFacade.Services.ExpenseService.SaveSingle(request);

                //if (int.Parse(collection["apptype"].ToString().Trim()) == 0 || int.Parse(collection["apptype"].ToString().Trim()) == 1 || int.Parse(collection["apptype"].ToString().Trim()) == 2 || int.Parse(collection["apptype"].ToString().Trim()) == 3 || int.Parse(collection["apptype"].ToString().Trim()) == 4)
                //{
                #region Send Expense to Approve
                CaseAccountCollection allAccounts = THOSWEB.WebApiFacade.Services.ExpenseService.CaseAccountGetAll();
                CaseAccount advanceAccount = (from a in allAccounts.List where a.AccountID == SessionManager.Current.CurrentUser.AdvanceAccount.AccountID select a).FirstOrDefault<CaseAccount>();
                CaseAccount caseAccount = (from c in allAccounts.List where c.AccountID == SessionManager.Current.CurrentUser.CaseAccount.AccountID select c).FirstOrDefault<CaseAccount>();


                // güncelleme olmayacak
                //if (request.ExpenseID > 0)
                //{
                //    double expenseGeneralAmount = request.GeneralAmount; ;

                //    if (advanceAccount.Balance > 0 && advanceAccount.Balance < expenseGeneralAmount)
                //    {
                //        request.UndocumentedAmount = 0;
                //        request.DocumentedAmount = advanceAccount.Balance;
                //        request.TotalAmount = advanceAccount.Balance;
                //        request.GeneralAmount = advanceAccount.Balance;
                //        request.Account = SessionManager.Current.CurrentUser.AdvanceAccount;
                //    }
                //    else
                //        expenseGeneralAmount = 0;

                //    THOSWEB.WebApiFacade.WebApiCaller.SetRequesterAdditionalParameters("expenseMethodName", collection["expenseMethodName"]);
                //    _result = THOSWEB.WebApiFacade.Services.ExpenseService.SaveSingle(request);
                //    var isChangedDocumnet = collection["isExpenseDocumentChanged"];
                //    //if (Convert.ToBoolean(isChangedDocumnet))
                //    //    saveExpenseDocument(file, _userContext, _result);

                //    foreach(HttpPostedFileBase file in divitFile){
                //        saveExpenseDocument(file, _userContext, _result, divitGuid);
                //    }

                //    return RedirectToAction("GetExpense", "Expenses", new { relatedCustomerID = request.RelatedCustomer.CustomerID, relatedexpenseid = int.Parse(collection["relatedexpenseid"].ToString()), type = int.Parse(collection["RelatedApplicationType"].ToString()), appid = int.Parse(collection["RelatedApplicationID"].ToString()), baseappid = int.Parse(collection["baseappid"].ToString()), success = 1, apptype = int.Parse(collection["apptype"].ToString().Trim()), mainentitytype = int.Parse(collection["mainentitytype"].ToString().Trim()) });
                //}

                //Checkexpense Delegation
                CheckExpenseDelegation();

                //int fileRelatedCustomerID = 0;
                if (request.RelatedApplicationType == (int)Enumerations.Expense.RelatedApplicationType.Levy)
                {
                    ObjectModelLibrary.Levy relatedLevy = THOSWEB.WebApiFacade.Services.LevyService.GetSingleLevy(SessionManager.Current.CurrentUser.UserID, request.RelatedApplicationID, 360, null, null, null, null);

                    var customerverification = THOSWEB.WebApiFacade.Services.BankingService.CustomerVerification(relatedLevy.CustomerNumber);

                    if (!customerverification)
                    {
                        return RedirectToAction("GetExpense", "Expenses", new { relatedCustomerID = int.Parse(collection["CustomersExpense"]).ToString(), relatedexpenseid = int.Parse(collection["relatedexpenseid"].ToString()), type = int.Parse(collection["RelatedApplicationType"].ToString()), appid = int.Parse(collection["RelatedApplicationID"].ToString()), baseappid = int.Parse(collection["baseappid"].ToString()), success = 5, apptype = int.Parse(collection["apptype"].ToString().Trim()), mainentitytype = int.Parse(collection["mainentitytype"].ToString().Trim()) });
                    }

                    if (string.IsNullOrEmpty(relatedLevy.CustomerNumber))
                    {
                        return RedirectToAction("GetExpense", "Expenses", new { relatedCustomerID = int.Parse(collection["CustomersExpense"]).ToString(), relatedexpenseid = int.Parse(collection["relatedexpenseid"].ToString()), type = int.Parse(collection["RelatedApplicationType"].ToString()), appid = int.Parse(collection["RelatedApplicationID"].ToString()), baseappid = int.Parse(collection["baseappid"].ToString()), success = 3, apptype = int.Parse(collection["apptype"].ToString().Trim()), mainentitytype = int.Parse(collection["mainentitytype"].ToString().Trim()) });
                    }

                    if (THOSWEB.WebApiFacade.Services.UserCommonService.GetSingleUser(relatedLevy.Advocate.UserID).LawFirm.IsMainFirm)
                    {
                        if (!CacheManager.IsIntegrationActive)
                        {
                            THOSWEB.WebApiFacade.WebApiCaller.SetRequesterAdditionalParameters("expenseMethodName", collection["expenseMethodName"]);
                            logger.Info("Levy Expense Save... (1)");

                            _result = THOSWEB.WebApiFacade.Services.ExpenseService.SaveSingle(request);

                            logger.Info("Levy Expense Document Save... (1)");
                            //saveExpenseDocument(file, _userContext, _result);

                            string[] documentIdArray = new string[expenseItemList.Count];
                            string[] documentFileNameArray = new string[expenseItemList.Count];
                            int arrayIndex = 0;
                            //Divitleri kaydetme işlemi
                            foreach (HttpPostedFileBase file in divitFile)
                            {
                                if (file != null)
                                {
                                    Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                    documentIdArray[arrayIndex] = document.DocumentUniqueID;
                                    documentFileNameArray[arrayIndex] = Path.GetFileNameWithoutExtension(file.FileName);
                                    arrayIndex++;
                                }
                            }
                            arrayIndex = 0;
                            logger.Info("Levy Expense Item Save... (1)");
                            //Gider Kalemleri kayıt işlemi
                            if (!DataHelper.GetBool(collection["IsBankingFile"], false, false))
                            {
                                foreach (ExpenseItem expenseItem in expenseItemList)
                                {
                                    expenseItem.ParentID = _result.ExpenseID;
                                    expenseItem.DocumentUniqueID = documentIdArray[arrayIndex];
                                    expenseItem.DocumentFileName = documentFileNameArray[arrayIndex];
                                    arrayIndex++;
                                    THOSWEB.WebApiFacade.Services.ExpenseService.InsertExpenseItem(expenseItem);
                                }
                            }

                            logger.Info("Levy Expense Approve Step Add... (1)");
                            if (!SessionManager.Current.CurrentUser.LawFirm.IsMainFirm)
                                THOSWEB.WebApiFacade.Services.UtilityService.NewLawFirmExpenseApprove(_result, relatedLevy.Advocate.UserID);
                            else
                                THOSWEB.WebApiFacade.Services.UtilityService.NewBankAdvocateExpenseApprove(_result, SessionManager.Current.CurrentUser.UserID);
                        }
                        else
                        {
                            double expenseGeneralAmount = request.GeneralAmount;

                            if (advanceAccount == null)
                            {
                                return RedirectToAction("GetExpense", "Expenses", new { relatedCustomerID = int.Parse(collection["CustomersExpense"]).ToString(), relatedexpenseid = int.Parse(collection["relatedexpenseid"].ToString()), type = int.Parse(collection["RelatedApplicationType"].ToString()), appid = int.Parse(collection["RelatedApplicationID"].ToString()), baseappid = int.Parse(collection["baseappid"].ToString()), success = 6, apptype = int.Parse(collection["apptype"].ToString().Trim()), mainentitytype = int.Parse(collection["mainentitytype"].ToString().Trim()) });
                            }

                            if (advanceAccount.Balance > 0 && advanceAccount.Balance < expenseGeneralAmount)
                            {
                                request.UndocumentedAmount = 0;
                                request.DocumentedAmount = advanceAccount.Balance;
                                request.TotalAmount = advanceAccount.Balance;
                                request.Account = SessionManager.Current.CurrentUser.AdvanceAccount;
                            }
                            else
                                expenseGeneralAmount = 0;
                            THOSWEB.WebApiFacade.WebApiCaller.SetRequesterAdditionalParameters("expenseMethodName", collection["expenseMethodName"]);

                            logger.Info("Levy Expense Save... (2)");
                            _result = THOSWEB.WebApiFacade.Services.ExpenseService.SaveSingle(request);


                            //Entegrasyon Dosyası Değilse
                            if (!DataHelper.GetBool(collection["IsBankingFile"], false, false))
                            {
                                string[] documentIdArray = new string[expenseItemList.Count];
                                string[] documentFileNameArray = new string[expenseItemList.Count];
                                int arrayIndex = 0;

                                logger.Info("Levy Expense Document Save... (2)");
                                //Divitleri kaydetme işlemi
                                foreach (HttpPostedFileBase file in divitFile)
                                {
                                    if (file != null)
                                    {
                                        Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                        documentIdArray[arrayIndex] = document.DocumentUniqueID;
                                        documentFileNameArray[arrayIndex] = Path.GetFileNameWithoutExtension(file.FileName);
                                        arrayIndex++;

                                    }
                                }
                                arrayIndex = 0;
                                logger.Info("Levy Expense Item Save... (1)");
                                //Gider Kalemleri kayıt işlemi

                                foreach (ExpenseItem expenseItem in expenseItemList)
                                {
                                    expenseItem.ParentID = _result.ExpenseID;
                                    expenseItem.DocumentUniqueID = documentIdArray[arrayIndex];
                                    expenseItem.DocumentFileName = documentFileNameArray[arrayIndex];
                                    arrayIndex++;
                                    THOSWEB.WebApiFacade.Services.ExpenseService.InsertExpenseItem(expenseItem);

                                }
                            }//Entegrasyon Dosyası ise
                            else
                            {
                                logger.Info("Levy Expense Document Save... (2)");
                                foreach (HttpPostedFileBase file in divitFile)
                                {
                                    if (file != null)
                                    {
                                        Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                    }
                                }
                            }

                            logger.Info("Levy Expense Approve Step Add... (2)");
                            if (!SessionManager.Current.CurrentUser.LawFirm.IsMainFirm)
                                THOSWEB.WebApiFacade.Services.UtilityService.NewLawFirmExpenseApprove(_result, relatedLevy.Advocate.UserID);
                            else
                                THOSWEB.WebApiFacade.Services.UtilityService.NewBankAdvocateExpenseApprove(_result, SessionManager.Current.CurrentUser.UserID);


                            logger.Info("Levy Expense Again Save...");
                            if (expenseGeneralAmount > 0)
                            {
                                _result.ExpenseID = 0;
                                _result.UndocumentedAmount = 0;
                                _result.DocumentedAmount = expenseGeneralAmount - advanceAccount.Balance;
                                _result.TotalAmount = expenseGeneralAmount - advanceAccount.Balance;
                                _result.Account = SessionManager.Current.CurrentUser.CaseAccount;
                                THOSWEB.WebApiFacade.WebApiCaller.SetRequesterAdditionalParameters("expenseMethodName", collection["expenseMethodName"]);

                                _result = THOSWEB.WebApiFacade.Services.ExpenseService.SaveSingle(_result);

                                //Entegrasyon Dosyası Değilse
                                if (!DataHelper.GetBool(collection["IsBankingFile"], false, false))
                                {
                                    string[] documentIdArray = new string[expenseItemList.Count];
                                    string[] documentFileNameArray = new string[expenseItemList.Count];
                                    int arrayIndex = 0;

                                    logger.Info("Levy Expense Document Save... (2)");
                                    //Divitleri kaydetme işlemi
                                    foreach (HttpPostedFileBase file in divitFile)
                                    {
                                        if (file != null)
                                        {
                                            Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                            documentIdArray[arrayIndex] = document.DocumentUniqueID;
                                            documentFileNameArray[arrayIndex] = Path.GetFileNameWithoutExtension(file.FileName);
                                            arrayIndex++;

                                        }
                                    }
                                    arrayIndex = 0;
                                    logger.Info("Levy Expense Item Save... (1)");
                                    //Gider Kalemleri kayıt işlemi

                                    foreach (ExpenseItem expenseItem in expenseItemList)
                                    {
                                        expenseItem.ParentID = _result.ExpenseID;
                                        expenseItem.DocumentUniqueID = documentIdArray[arrayIndex];
                                        expenseItem.DocumentFileName = documentFileNameArray[arrayIndex];
                                        arrayIndex++;
                                        THOSWEB.WebApiFacade.Services.ExpenseService.InsertExpenseItem(expenseItem);

                                    }
                                }//Entegrasyon Dosyası ise
                                else
                                {
                                    logger.Info("Levy Expense Document Save... (2)");
                                    foreach (HttpPostedFileBase file in divitFile)
                                    {
                                        if (file != null)
                                        {
                                            Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                        }
                                    }
                                }

                                if (!SessionManager.Current.CurrentUser.LawFirm.IsMainFirm)
                                    THOSWEB.WebApiFacade.Services.UtilityService.NewLawFirmExpenseApprove(_result, relatedLevy.Advocate.UserID);
                                else
                                    THOSWEB.WebApiFacade.Services.UtilityService.NewBankAdvocateExpenseApprove(_result, SessionManager.Current.CurrentUser.UserID);
                            }
                        }
                    }
                    else
                    {
                        return RedirectToAction("GetExpense", "Expenses", new { relatedCustomerID = int.Parse(collection["CustomersExpense"]).ToString(), relatedexpenseid = int.Parse(collection["relatedexpenseid"].ToString()), type = int.Parse(collection["RelatedApplicationType"].ToString()), appid = int.Parse(collection["RelatedApplicationID"].ToString()), baseappid = int.Parse(collection["baseappid"].ToString()), success = 4, apptype = int.Parse(collection["apptype"].ToString().Trim()), mainentitytype = int.Parse(collection["mainentitytype"].ToString().Trim()) });
                        //Helper.ShowMessageBox("Masraf eklemek istediğiniz dosyanın Avukatı Banka avukatı olarak tanımlanmamış ! Banka avukatı tanımlanmamış dosyalara masraf ekleyemezsiniz.", "Dosyaya Banka Avukatı tanımlanmamış", MessageBoxButtons.OK, RadMessageIcon.Question);
                    }

                }
                else if (request.RelatedApplicationType == (int)Enumerations.Expense.RelatedApplicationType.Law)
                {
                    //ObjectModelLibrary.Law relatedLaw = new ObjectModelLibrary.Law();
                    //relatedLaw.LawID = request.RelatedApplicationID;
                    //relatedLaw.BaseProps = new BaseProperties();
                    //relatedLaw.BaseProps.RequesterUserInfo = SessionManager.Current.CurrentUser;
                    //relatedLaw = THOSWEB.WebApiFacade.Services.LawService.GetLaw(relatedLaw);

                    //fileRelatedCustomerID = (from l in relatedLaw.Defendant.List
                    //                         where l.IsCustomer
                    //                         && l.CustomerContext.CustomerID > 0
                    //                         select l.CustomerContext.CustomerID).FirstOrDefault();




                    if (string.IsNullOrEmpty(_law.CustomerNumber))
                    {
                        return RedirectToAction("GetExpense", "Expenses", new { relatedCustomerID = int.Parse(collection["CustomersExpense"]).ToString(), relatedexpenseid = int.Parse(collection["relatedexpenseid"].ToString()), type = int.Parse(collection["RelatedApplicationType"].ToString()), appid = int.Parse(collection["RelatedApplicationID"].ToString()), baseappid = int.Parse(collection["baseappid"].ToString()), success = 3, apptype = int.Parse(collection["apptype"].ToString().Trim()), mainentitytype = int.Parse(collection["mainentitytype"].ToString().Trim()) });
                    }

                    var customerverification = THOSWEB.WebApiFacade.Services.BankingService.CustomerVerification(_law.CustomerNumber);

                    if (!customerverification)
                    {
                        return RedirectToAction("GetExpense", "Expenses", new { relatedCustomerID = int.Parse(collection["CustomersExpense"]).ToString(), relatedexpenseid = int.Parse(collection["relatedexpenseid"].ToString()), type = int.Parse(collection["RelatedApplicationType"].ToString()), appid = int.Parse(collection["RelatedApplicationID"].ToString()), baseappid = int.Parse(collection["baseappid"].ToString()), success = 5, apptype = int.Parse(collection["apptype"].ToString().Trim()), mainentitytype = int.Parse(collection["mainentitytype"].ToString().Trim()) });
                    }

                    if (THOSWEB.WebApiFacade.Services.UserCommonService.GetSingleUser(_law.Advocate.UserID).LawFirm.IsMainFirm)
                    {
                        if (!CacheManager.IsIntegrationActive)
                        {
                            THOSWEB.WebApiFacade.WebApiCaller.SetRequesterAdditionalParameters("expenseMethodName", collection["expenseMethodName"]);

                            _result = THOSWEB.WebApiFacade.Services.ExpenseService.SaveSingle(request);

                            //Entegrasyon Dosyası Değilse
                            if (!DataHelper.GetBool(collection["IsBankingFile"], false, false))
                            {
                                string[] documentIdArray = new string[expenseItemList.Count];
                                string[] documentFileNameArray = new string[expenseItemList.Count];
                                int arrayIndex = 0;

                                logger.Info("Levy Expense Document Save... (2)");
                                //Divitleri kaydetme işlemi
                                foreach (HttpPostedFileBase file in divitFile)
                                {
                                    if (file != null)
                                    {
                                        Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                        documentIdArray[arrayIndex] = document.DocumentUniqueID;
                                        documentFileNameArray[arrayIndex] = Path.GetFileNameWithoutExtension(file.FileName);
                                        arrayIndex++;

                                    }
                                }
                                arrayIndex = 0;
                                logger.Info("Levy Expense Item Save... (1)");
                                //Gider Kalemleri kayıt işlemi

                                foreach (ExpenseItem expenseItem in expenseItemList)
                                {
                                    expenseItem.ParentID = _result.ExpenseID;
                                    expenseItem.DocumentUniqueID = documentIdArray[arrayIndex];
                                    expenseItem.DocumentFileName = documentFileNameArray[arrayIndex];
                                    arrayIndex++;
                                    THOSWEB.WebApiFacade.Services.ExpenseService.InsertExpenseItem(expenseItem);

                                }
                            }//Entegrasyon Dosyası ise
                            else
                            {
                                logger.Info("Levy Expense Document Save... (2)");
                                foreach (HttpPostedFileBase file in divitFile)
                                {
                                    if (file != null)
                                    {
                                        Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                    }
                                }
                            }

                            if (!SessionManager.Current.CurrentUser.LawFirm.IsMainFirm)
                                THOSWEB.WebApiFacade.Services.UtilityService.NewLawFirmExpenseApprove(_result, _law.Advocate.UserID);
                            else
                                THOSWEB.WebApiFacade.Services.UtilityService.NewBankAdvocateExpenseApprove(_result, SessionManager.Current.CurrentUser.UserID);
                        }
                        else
                        {
                            double expenseTotalAmount = request.TotalAmount; ;

                            if (advanceAccount.Balance > 0 && advanceAccount.Balance < expenseTotalAmount)
                            {
                                request.UndocumentedAmount = 0;
                                request.DocumentedAmount = advanceAccount.Balance;
                                request.TotalAmount = advanceAccount.Balance;
                                request.Account = SessionManager.Current.CurrentUser.AdvanceAccount;
                            }
                            else
                                expenseTotalAmount = 0;
                            THOSWEB.WebApiFacade.WebApiCaller.SetRequesterAdditionalParameters("expenseMethodName", collection["expenseMethodName"]);

                            _result = THOSWEB.WebApiFacade.Services.ExpenseService.SaveSingle(request);

                            //Entegrasyon Dosyası Değilse
                            if (!DataHelper.GetBool(collection["IsBankingFile"], false, false))
                            {
                                string[] documentIdArray = new string[expenseItemList.Count];
                                string[] documentFileNameArray = new string[expenseItemList.Count];
                                int arrayIndex = 0;

                                logger.Info("Levy Expense Document Save... (2)");
                                //Divitleri kaydetme işlemi
                                foreach (HttpPostedFileBase file in divitFile)
                                {
                                    if (file != null)
                                    {
                                        Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                        documentIdArray[arrayIndex] = document.DocumentUniqueID;
                                        documentFileNameArray[arrayIndex] = Path.GetFileNameWithoutExtension(file.FileName);
                                        arrayIndex++;

                                    }
                                }
                                arrayIndex = 0;
                                logger.Info("Levy Expense Item Save... (1)");
                                //Gider Kalemleri kayıt işlemi

                                foreach (ExpenseItem expenseItem in expenseItemList)
                                {
                                    expenseItem.ParentID = _result.ExpenseID;
                                    expenseItem.DocumentUniqueID = documentIdArray[arrayIndex];
                                    expenseItem.DocumentFileName = documentFileNameArray[arrayIndex];
                                    arrayIndex++;
                                    THOSWEB.WebApiFacade.Services.ExpenseService.InsertExpenseItem(expenseItem);

                                }
                            }//Entegrasyon Dosyası ise
                            else
                            {
                                logger.Info("Levy Expense Document Save... (2)");
                                foreach (HttpPostedFileBase file in divitFile)
                                {
                                    if (file != null)
                                    {
                                        Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                    }
                                }
                            }
                            if (!SessionManager.Current.CurrentUser.LawFirm.IsMainFirm)
                                THOSWEB.WebApiFacade.Services.UtilityService.NewLawFirmExpenseApprove(_result, _law.Advocate.UserID);
                            else
                                THOSWEB.WebApiFacade.Services.UtilityService.NewBankAdvocateExpenseApprove(_result, SessionManager.Current.CurrentUser.UserID);

                            if (expenseTotalAmount > 0)
                            {
                                _result.ExpenseID = 0;
                                _result.UndocumentedAmount = 0;
                                _result.DocumentedAmount = expenseTotalAmount - advanceAccount.Balance;
                                _result.TotalAmount = expenseTotalAmount - advanceAccount.Balance;
                                _result.Account = SessionManager.Current.CurrentUser.CaseAccount;

                                _result = THOSWEB.WebApiFacade.Services.ExpenseService.SaveSingle(_result);

                                //Entegrasyon Dosyası Değilse
                                if (!DataHelper.GetBool(collection["IsBankingFile"], false, false))
                                {
                                    string[] documentIdArray = new string[expenseItemList.Count];
                                    string[] documentFileNameArray = new string[expenseItemList.Count];
                                    int arrayIndex = 0;

                                    logger.Info("Levy Expense Document Save... (2)");
                                    //Divitleri kaydetme işlemi
                                    foreach (HttpPostedFileBase file in divitFile)
                                    {
                                        if (file != null)
                                        {
                                            Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                            documentIdArray[arrayIndex] = document.DocumentUniqueID;
                                            documentFileNameArray[arrayIndex] = Path.GetFileNameWithoutExtension(file.FileName);
                                            arrayIndex++;

                                        }
                                    }
                                    arrayIndex = 0;
                                    logger.Info("Levy Expense Item Save... (1)");
                                    //Gider Kalemleri kayıt işlemi

                                    foreach (ExpenseItem expenseItem in expenseItemList)
                                    {
                                        expenseItem.ParentID = _result.ExpenseID;
                                        expenseItem.DocumentUniqueID = documentIdArray[arrayIndex];
                                        expenseItem.DocumentFileName = documentFileNameArray[arrayIndex];
                                        arrayIndex++;
                                        THOSWEB.WebApiFacade.Services.ExpenseService.InsertExpenseItem(expenseItem);

                                    }
                                }//Entegrasyon Dosyası ise
                                else
                                {
                                    logger.Info("Levy Expense Document Save... (2)");
                                    foreach (HttpPostedFileBase file in divitFile)
                                    {
                                        if (file != null)
                                        {
                                            Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                        }
                                    }
                                }

                                if (!SessionManager.Current.CurrentUser.LawFirm.IsMainFirm)
                                    THOSWEB.WebApiFacade.Services.UtilityService.NewLawFirmExpenseApprove(_result, _law.Advocate.UserID);
                                else
                                    THOSWEB.WebApiFacade.Services.UtilityService.NewBankAdvocateExpenseApprove(_result, SessionManager.Current.CurrentUser.UserID);
                            }
                        }
                    }
                    else
                    {
                        return RedirectToAction("GetExpense", "Expenses", new { relatedCustomerID = int.Parse(collection["CustomersExpense"]).ToString(), relatedexpenseid = int.Parse(collection["relatedexpenseid"].ToString()), type = int.Parse(collection["RelatedApplicationType"].ToString()), appid = int.Parse(collection["RelatedApplicationID"].ToString()), baseappid = int.Parse(collection["baseappid"].ToString()), success = 4, apptype = int.Parse(collection["apptype"].ToString().Trim()), mainentitytype = int.Parse(collection["mainentitytype"].ToString().Trim()) });
                    }

                }
                else if (request.RelatedApplicationType == (int)Enumerations.Expense.RelatedApplicationType.General)
                {
                    //if (SessionManager.Current.CurrentUser.LawFirm.IsMainFirm)
                    //{
                    THOSWEB.WebApiFacade.WebApiCaller.SetRequesterAdditionalParameters("expenseMethodName", collection["expenseMethodName"]);

                    _result = THOSWEB.WebApiFacade.Services.ExpenseService.SaveSingle(request);

                    //Entegrasyon Dosyası Değilse
                    if (!DataHelper.GetBool(collection["IsBankingFile"], false, false))
                    {
                        string[] documentIdArray = new string[expenseItemList.Count];
                        string[] documentFileNameArray = new string[expenseItemList.Count];
                        int arrayIndex = 0;

                        logger.Info("Levy Expense Document Save... (2)");
                        //Divitleri kaydetme işlemi
                        foreach (HttpPostedFileBase file in divitFile)
                        {
                            if (file != null)
                            {
                                Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                documentIdArray[arrayIndex] = document.DocumentUniqueID;
                                documentFileNameArray[arrayIndex] = Path.GetFileNameWithoutExtension(file.FileName);
                                arrayIndex++;

                            }
                        }
                        arrayIndex = 0;
                        logger.Info("Levy Expense Item Save... (1)");
                        //Gider Kalemleri kayıt işlemi

                        foreach (ExpenseItem expenseItem in expenseItemList)
                        {
                            expenseItem.ParentID = _result.ExpenseID;
                            expenseItem.DocumentUniqueID = documentIdArray[arrayIndex];
                            expenseItem.DocumentFileName = documentFileNameArray[arrayIndex];
                            arrayIndex++;
                            THOSWEB.WebApiFacade.Services.ExpenseService.InsertExpenseItem(expenseItem);

                        }
                    }//Entegrasyon Dosyası ise
                    else
                    {
                        logger.Info("Levy Expense Document Save... (2)");
                        foreach (HttpPostedFileBase file in divitFile)
                        {
                            if (file != null)
                            {
                                Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                            }
                        }
                    }
                    THOSWEB.WebApiFacade.Services.UtilityService.NewBankAdvocateExpenseApprove(_result, SessionManager.Current.CurrentUser.UserID);

                    SessionManager.Current.SetValue("latestGeneralExpenseRecordID", request.ExpenseID);
                    //}
                }
                else if (request.RelatedApplicationType == (int)Enumerations.Expense.RelatedApplicationType.Issue)
                {
                    THOSWEB.WebApiFacade.WebApiCaller.SetRequesterAdditionalParameters("expenseMethodName", collection["expenseMethodName"]);

                    _result = THOSWEB.WebApiFacade.Services.ExpenseService.SaveSingle(request);

                    //Entegrasyon Dosyası Değilse
                    if (!DataHelper.GetBool(collection["IsBankingFile"], false, false))
                    {
                        string[] documentIdArray = new string[expenseItemList.Count];
                        string[] documentFileNameArray = new string[expenseItemList.Count];
                        int arrayIndex = 0;

                        logger.Info("Levy Expense Document Save... (2)");
                        //Divitleri kaydetme işlemi
                        foreach (HttpPostedFileBase file in divitFile)
                        {
                            if (file != null)
                            {
                                Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                documentIdArray[arrayIndex] = document.DocumentUniqueID;
                                documentFileNameArray[arrayIndex] = Path.GetFileNameWithoutExtension(file.FileName);
                                arrayIndex++;

                            }
                        }
                        arrayIndex = 0;
                        logger.Info("Levy Expense Item Save... (1)");
                        //Gider Kalemleri kayıt işlemi

                        foreach (ExpenseItem expenseItem in expenseItemList)
                        {
                            expenseItem.ParentID = _result.ExpenseID;
                            expenseItem.DocumentUniqueID = documentIdArray[arrayIndex];
                            expenseItem.DocumentFileName = documentFileNameArray[arrayIndex];
                            arrayIndex++;
                            THOSWEB.WebApiFacade.Services.ExpenseService.InsertExpenseItem(expenseItem);

                        }
                    }//Entegrasyon Dosyası ise
                    else
                    {
                        logger.Info("Levy Expense Document Save... (2)");
                        foreach (HttpPostedFileBase file in divitFile)
                        {
                            if (file != null)
                            {
                                Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                            }
                        }
                    }

                    THOSWEB.WebApiFacade.Services.UtilityService.NewBankAdvocateExpenseApprove(_result, SessionManager.Current.CurrentUser.UserID);

                    SessionManager.Current.SetValue("latestGeneralExpenseRecordID", request.ExpenseID);


                    return RedirectToAction("IssueDetail", "Consultation", new { issueId = _result.RelatedApplicationID });
                }
                else if (request.RelatedApplicationType == (int)Enumerations.Expense.RelatedApplicationType.DocumenFlow)
                {
                    THOSWEB.WebApiFacade.WebApiCaller.SetRequesterAdditionalParameters("expenseMethodName", collection["expenseMethodName"]);

                    _result = THOSWEB.WebApiFacade.Services.ExpenseService.SaveSingle(request);

                    //Entegrasyon Dosyası Değilse
                    if (!DataHelper.GetBool(collection["IsBankingFile"], false, false))
                    {
                        string[] documentIdArray = new string[expenseItemList.Count];
                        string[] documentFileNameArray = new string[expenseItemList.Count];
                        int arrayIndex = 0;

                        logger.Info("Levy Expense Document Save... (2)");
                        //Divitleri kaydetme işlemi
                        foreach (HttpPostedFileBase file in divitFile)
                        {
                            if (file != null)
                            {
                                Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                                documentIdArray[arrayIndex] = document.DocumentUniqueID;
                                documentFileNameArray[arrayIndex] = Path.GetFileNameWithoutExtension(file.FileName);
                                arrayIndex++;

                            }
                        }
                        arrayIndex = 0;
                        logger.Info("Levy Expense Item Save... (1)");
                        //Gider Kalemleri kayıt işlemi

                        foreach (ExpenseItem expenseItem in expenseItemList)
                        {
                            expenseItem.ParentID = _result.ExpenseID;
                            expenseItem.DocumentUniqueID = documentIdArray[arrayIndex];
                            expenseItem.DocumentFileName = documentFileNameArray[arrayIndex];
                            arrayIndex++;
                            THOSWEB.WebApiFacade.Services.ExpenseService.InsertExpenseItem(expenseItem);

                        }
                    }//Entegrasyon Dosyası ise
                    else
                    {
                        logger.Info("Levy Expense Document Save... (2)");
                        foreach (HttpPostedFileBase file in divitFile)
                        {
                            if (file != null)
                            {
                                Document document = saveExpenseDocument(file, _userContext, _result, divitGuid);
                            }
                        }
                    }
                    THOSWEB.WebApiFacade.Services.UtilityService.NewBankAdvocateExpenseApprove(_result, SessionManager.Current.CurrentUser.UserID);

                    SessionManager.Current.SetValue("latestGeneralExpenseRecordID", request.ExpenseID);

                    return RedirectToAction("DocumentFlowDetail", "DocumentFlow", new { documentflowid = _result.RelatedApplicationID });
                }
                #endregion


                return RedirectToAction("GetExpense", "Expenses", new { relatedCustomerID = request.RelatedCustomer.CustomerID, relatedexpenseid = int.Parse(collection["relatedexpenseid"].ToString()), type = int.Parse(collection["RelatedApplicationType"].ToString()), appid = int.Parse(collection["RelatedApplicationID"].ToString()), baseappid = int.Parse(collection["baseappid"].ToString()), success = 1, apptype = int.Parse(collection["apptype"].ToString().Trim()), mainentitytype = int.Parse(collection["mainentitytype"].ToString().Trim()) });


            }
            catch (Exception ex)
            {
                if (_result != null && _result.ExpenseID > 0)
                {
                    THOSWEB.WebApiFacade.Services.ExpenseService.DeleteSingle(_result.ExpenseID);
                    DeleteDocument(_result);
                }

                simpleLogger.Error("ActionName :AddupdateExpense \n InnerException :" + (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : string.Empty) + "Exception :" + ex.ToString());

                SessionManager.Current.SetValue("ExpenseError", ex != null ? (ex.Message ?? string.Empty) : string.Empty);
                return RedirectToAction("GetExpense", "Expenses", new { relatedCustomerID = int.Parse(collection["CustomersExpense"]).ToString(), relatedexpenseid = int.Parse(collection["relatedexpenseid"].ToString()), type = int.Parse(collection["RelatedApplicationType"].ToString()), appid = int.Parse(collection["RelatedApplicationID"].ToString()), baseappid = int.Parse(collection["baseappid"].ToString()), success = 2, apptype = int.Parse(collection["apptype"].ToString().Trim()), mainentitytype = int.Parse(collection["mainentitytype"].ToString().Trim()) });
            }

        }