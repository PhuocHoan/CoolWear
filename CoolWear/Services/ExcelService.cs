using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using CoolWear.Models;
using System.Linq;
using System;

namespace CoolWear.Services
{
    public class ExcelService
    {
        /// <summary>
        /// Imports a list of ProductColor from an Excel file.
        /// </summary>
        /// <param name="filePath">The path to the Excel file.</param>
        /// <returns>A list of ProductColor objects.</returns>
        public List<ProductColor> ImportColorsFromExcel(string filePath)
        {
            var colors = new List<ProductColor>();

            using (var document = SpreadsheetDocument.Open(filePath, false))
            {
                var workbookPart = document.WorkbookPart;
                var sheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();
                if (sheet == null) return colors;

                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var rows = worksheetPart.Worksheet.Descendants<Row>().Skip(1); // Skip header row

                foreach (var row in rows)
                {
                    var cells = row.Elements<Cell>().ToArray();
                    var color = new ProductColor
                    {
                        ColorId = int.TryParse(GetCellValue(workbookPart, cells[0]), out var colorId) ? colorId : 0,
                        ColorName = GetCellValue(workbookPart, cells[1]),
                        IsDeleted = bool.TryParse(GetCellValue(workbookPart, cells[2]), out var isDeleted) && isDeleted
                    };
                    colors.Add(color);
                }
            }

            return colors;
        }

        /// <summary>
        /// Exports a list of ProductColor to an Excel file.
        /// </summary>
        /// <param name="filePath">The path to save the Excel file.</param>
        /// <param name="colors">The list of ProductColor objects to export.</param>
        public void ExportColorsToExcel(string filePath, List<ProductColor> colors)
        {
            if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".xlsx";
            }

            using (var document = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet
                {
                    Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Colors"
                };
                sheets.Append(sheet);

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add header row
                var headerRow = new Row();
                headerRow.Append(
                    CreateCell("ColorId"),
                    CreateCell("ColorName"),
                    CreateCell("IsDeleted")
                );
                sheetData.Append(headerRow);

                // Add data rows
                foreach (var color in colors)
                {
                    var dataRow = new Row();
                    dataRow.Append(
                        CreateCell(color.ColorId.ToString()),
                        CreateCell(color.ColorName),
                        CreateCell(color.IsDeleted.ToString())
                    );
                    sheetData.Append(dataRow);
                }

                workbookPart.Workbook.Save();
            }
        }

        /// <summary>
        /// Imports a list of ProductSize from an Excel file.
        /// </summary>
        /// <param name="filePath">The path to the Excel file.</param>
        /// <returns>A list of ProductSize objects.</returns>
        public List<ProductSize> ImportSizesFromExcel(string filePath)
        {
            var sizes = new List<ProductSize>();

            using (var document = SpreadsheetDocument.Open(filePath, false))
            {
                var workbookPart = document.WorkbookPart;
                var sheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();
                if (sheet == null) return sizes;

                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var rows = worksheetPart.Worksheet.Descendants<Row>().Skip(1); // Skip header row

                foreach (var row in rows)
                {
                    var cells = row.Elements<Cell>().ToArray();
                    var size = new ProductSize
                    {
                        SizeId = int.TryParse(GetCellValue(workbookPart, cells[0]), out var sizeId) ? sizeId : 0,
                        SizeName = GetCellValue(workbookPart, cells[1]),
                        IsDeleted = bool.TryParse(GetCellValue(workbookPart, cells[2]), out var isDeleted) && isDeleted
                    };
                    sizes.Add(size);
                }
            }

            return sizes;
        }

        /// <summary>
        /// Exports a list of ProductSize to an Excel file.
        /// </summary>
        /// <param name="filePath">The path to save the Excel file.</param>
        /// <param name="sizes">The list of ProductSize objects to export.</param>
        public void ExportSizesToExcel(string filePath, List<ProductSize> sizes)
        {
            if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".xlsx";
            }

            using (var document = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet
                {
                    Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Sizes"
                };
                sheets.Append(sheet);

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add header row
                var headerRow = new Row();
                headerRow.Append(
                    CreateCell("SizeId"),
                    CreateCell("SizeName"),
                    CreateCell("IsDeleted")
                );
                sheetData.Append(headerRow);

                // Add data rows
                foreach (var size in sizes)
                {
                    var dataRow = new Row();
                    dataRow.Append(
                        CreateCell(size.SizeId.ToString()),
                        CreateCell(size.SizeName),
                        CreateCell(size.IsDeleted.ToString())
                    );
                    sheetData.Append(dataRow);
                }

                workbookPart.Workbook.Save();
            }
        }

        /// <summary>
        /// Imports a list of ProductCategory from an Excel file.
        /// </summary>
        /// <param name="filePath">The path to the Excel file.</param>
        /// <returns>A list of ProductCategory objects.</returns>
        /// <summary>
        /// Imports a list of ProductCategory from an Excel file.
        /// </summary>
        /// <param name="filePath">The path to the Excel file.</param>
        /// <returns>A list of ProductCategory objects.</returns>
        public List<ProductCategory> ImportCategoriesFromExcel(string filePath)
        {
            var categories = new List<ProductCategory>();

            using (var document = SpreadsheetDocument.Open(filePath, false))
            {
                var workbookPart = document.WorkbookPart;
                var sheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();
                if (sheet == null) return categories;

                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var rows = worksheetPart.Worksheet.Descendants<Row>().Skip(1); // Skip header row

                foreach (var row in rows)
                {
                    var cells = row.Elements<Cell>().ToArray();
                    var category = new ProductCategory
                    {
                        CategoryId = int.TryParse(GetCellValue(workbookPart, cells[0]), out var categoryId) ? categoryId : 0,
                        CategoryName = GetCellValue(workbookPart, cells[1]),
                        ProductType = GetCellValue(workbookPart, cells[2])
                    };
                    categories.Add(category);
                }
            }

            return categories;
        }

        /// <summary>
        /// Exports a list of ProductCategory to an Excel file.
        /// </summary>
        /// <param name="filePath">The path to save the Excel file.</param>
        /// <param name="categories">The list of ProductCategory objects to export.</param>
        public void ExportCategoriesToExcel(string filePath, List<ProductCategory> categories)
        {
            if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".xlsx";
            }

            using (var document = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet
                {
                    Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Categories"
                };
                sheets.Append(sheet);

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add header row
                var headerRow = new Row();
                headerRow.Append(
                    CreateCell("CategoryId"),
                    CreateCell("CategoryName"),
                    CreateCell("ProductType")
                );
                sheetData.Append(headerRow);

                // Add data rows
                foreach (var category in categories)
                {
                    var dataRow = new Row();
                    dataRow.Append(
                        CreateCell(category.CategoryId.ToString()),
                        CreateCell(category.CategoryName),
                        CreateCell(category.ProductType)
                    );
                    sheetData.Append(dataRow);
                }

                workbookPart.Workbook.Save();
            }
        }

        /// <summary>
        /// Imports a list of Customer from an Excel file.
        /// </summary>
        /// <param name="filePath">The path to the Excel file.</param>
        /// <returns>A list of Customer objects.</returns>
        public List<Customer> ImportCustomersFromExcel(string filePath)
        {
            var customers = new List<Customer>();

            using (var document = SpreadsheetDocument.Open(filePath, false))
            {
                var workbookPart = document.WorkbookPart;
                var sheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();
                if (sheet == null) return customers;

                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var rows = worksheetPart.Worksheet.Descendants<Row>().Skip(1); // Skip header row

                foreach (var row in rows)
                {
                    var cells = row.Elements<Cell>().ToArray();
                    var customer = new Customer
                    {
                        CustomerId = int.TryParse(GetCellValue(workbookPart, cells[0]), out var customerId) ? customerId : 0,
                        CustomerName = GetCellValue(workbookPart, cells[1]),
                        Email = GetCellValue(workbookPart, cells[2]),
                        Phone = GetCellValue(workbookPart, cells[3]),
                        Address = GetCellValue(workbookPart, cells[4]),
                        CreateDate = DateTime.TryParse(GetCellValue(workbookPart, cells[5]), out var createDate) ? createDate : DateTime.Now,
                        Points = int.TryParse(GetCellValue(workbookPart, cells[6]), out var points) ? points : 0,
                        IsDeleted = bool.TryParse(GetCellValue(workbookPart, cells[7]), out var isDeleted) && isDeleted
                    };
                    customers.Add(customer);
                }
            }

            return customers;
        }

        /// <summary>
        /// Exports a list of Customer to an Excel file.
        /// </summary>
        /// <param name="filePath">The path to save the Excel file.</param>
        /// <param name="customers">The list of Customer objects to export.</param>
        public void ExportCustomersToExcel(string filePath, List<Customer> customers)
        {
            if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".xlsx";
            }

            using (var document = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet
                {
                    Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Customers"
                };
                sheets.Append(sheet);

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add header row
                var headerRow = new Row();
                headerRow.Append(
                    CreateCell("CustomerId"),
                    CreateCell("CustomerName"),
                    CreateCell("Email"),
                    CreateCell("Phone"),
                    CreateCell("Address"),
                    CreateCell("CreateDate"),
                    CreateCell("Points"),
                    CreateCell("IsDeleted")
                );
                sheetData.Append(headerRow);

                // Add data rows
                foreach (var customer in customers)
                {
                    var dataRow = new Row();
                    dataRow.Append(
                        CreateCell(customer.CustomerId.ToString()),
                        CreateCell(customer.CustomerName),
                        CreateCell(customer.Email ?? string.Empty),
                        CreateCell(customer.Phone),
                        CreateCell(customer.Address),
                        CreateCell(customer.CreateDate.ToString("yyyy-MM-dd")),
                        CreateCell(customer.Points.ToString()),
                        CreateCell(customer.IsDeleted.ToString())
                    );
                    sheetData.Append(dataRow);
                }

                workbookPart.Workbook.Save();
            }
        }

        public List<Product> ImportProductsFromExcel(string filePath)
        {
            var products = new List<Product>();

            using (var document = SpreadsheetDocument.Open(filePath, false))
            {
                var workbookPart = document.WorkbookPart;
                var sheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();
                if (sheet == null) return products;

                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var rows = worksheetPart.Worksheet.Descendants<Row>().Skip(1); // Skip header row

                foreach (var row in rows)
                {
                    var cells = row.Elements<Cell>().ToArray();

                    // Parse Product data
                    var productId = int.TryParse(GetCellValue(workbookPart, cells[0]), out var pid) ? pid : 0;
                    var productName = GetCellValue(workbookPart, cells[1]);
                    var importPrice = int.TryParse(GetCellValue(workbookPart, cells[2]), out var ip) ? ip : 0;
                    var price = int.TryParse(GetCellValue(workbookPart, cells[3]), out var p) ? p : 0;
                    var categoryId = int.TryParse(GetCellValue(workbookPart, cells[4]), out var cid) ? cid : (int?)null;
                    var publicId = GetCellValue(workbookPart, cells[5]);
                    var isDeleted = bool.TryParse(GetCellValue(workbookPart, cells[6]), out var del) && del;

                    // Check if the product already exists
                    var product = products.FirstOrDefault(p => p.ProductId == productId);
                    if (product == null)
                    {
                        product = new Product
                        {
                            ProductId = productId,
                            ProductName = productName,
                            ImportPrice = importPrice,
                            Price = price,
                            CategoryId = categoryId,
                            PublicId = publicId,
                            IsDeleted = isDeleted,
                            ProductVariants = new List<ProductVariant>()
                        };
                        products.Add(product);
                    }

                    // Parse ProductVariant data
                    var variantId = int.TryParse(GetCellValue(workbookPart, cells[7]), out var vid) ? vid : 0;
                    var colorId = int.TryParse(GetCellValue(workbookPart, cells[8]), out var colId) ? colId : (int?)null;
                    var sizeId = int.TryParse(GetCellValue(workbookPart, cells[9]), out var szId) ? szId : (int?)null;
                    var stockQuantity = int.TryParse(GetCellValue(workbookPart, cells[10]), out var sq) ? sq : 0;
                    var variantIsDeleted = bool.TryParse(GetCellValue(workbookPart, cells[11]), out var vdel) && vdel;

                    var variant = new ProductVariant
                    {
                        VariantId = variantId,
                        ProductId = productId,
                        ColorId = colorId,
                        SizeId = sizeId,
                        StockQuantity = stockQuantity,
                        IsDeleted = variantIsDeleted
                    };

                    product.ProductVariants.Add(variant);
                }
            }

            return products;
        }

        public void ExportProductsToExcel(string filePath, List<Product> products)
        {
            if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".xlsx";
            }

            using (var document = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet
                {
                    Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Products"
                };
                sheets.Append(sheet);

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add header row
                var headerRow = new Row();
                headerRow.Append(
                    CreateCell("ProductId"),
                    CreateCell("ProductName"),
                    CreateCell("ImportPrice"),
                    CreateCell("Price"),
                    CreateCell("CategoryId"),
                    CreateCell("PublicId"),
                    CreateCell("IsDeleted"),
                    CreateCell("VariantId"),
                    CreateCell("ColorId"),
                    CreateCell("SizeId"),
                    CreateCell("StockQuantity"),
                    CreateCell("VariantIsDeleted")
                );
                sheetData.Append(headerRow);

                // Add data rows
                foreach (var product in products)
                {
                    foreach (var variant in product.ProductVariants)
                    {
                        var dataRow = new Row();
                        dataRow.Append(
                            CreateCell(product.ProductId.ToString()),
                            CreateCell(product.ProductName),
                            CreateCell(product.ImportPrice.ToString()),
                            CreateCell(product.Price.ToString()),
                            CreateCell(product.CategoryId?.ToString() ?? string.Empty),
                            CreateCell(product.PublicId),
                            CreateCell(product.IsDeleted.ToString()),
                            CreateCell(variant.VariantId.ToString()),
                            CreateCell(variant.ColorId?.ToString() ?? string.Empty),
                            CreateCell(variant.SizeId?.ToString() ?? string.Empty),
                            CreateCell(variant.StockQuantity.ToString()),
                            CreateCell(variant.IsDeleted.ToString())
                        );
                        sheetData.Append(dataRow);
                    }
                }

                workbookPart.Workbook.Save();
            }
        }


        /// <summary>
        /// Helper method to get the value of a cell.
        /// </summary>
        private string GetCellValue(WorkbookPart workbookPart, Cell cell)
        {
            if (cell == null || cell.CellValue == null) return string.Empty;

            var value = cell.CellValue.Text;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                var sharedStringTable = workbookPart.SharedStringTablePart.SharedStringTable;
                return sharedStringTable.ElementAt(int.Parse(value)).InnerText;
            }

            return value;
        }

        /// <summary>
        /// Helper method to create a cell with a given value.
        /// </summary>
        private Cell CreateCell(string value)
        {
            return new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(value)
            };
        }
    }
}
