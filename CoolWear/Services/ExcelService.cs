using CoolWear.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoolWear.Services;

public class ExcelService
{
    /// <summary>
    /// Imports danh sách màu sắc từ file Excel.
    /// </summary>
    /// <param name="filePath">Đường dẫn tới file Excel.</param>
    /// <returns>Danh sách objects ProductColor.</returns>
    public static List<ProductColor>? ImportColorsFromExcel(string filePath)
    {
        var colors = new List<ProductColor>();

        using (var document = SpreadsheetDocument.Open(filePath, false))
        {
            var workbookPart = document.WorkbookPart;
            var sheet = workbookPart!.Workbook.Sheets!.Elements<Sheet>().FirstOrDefault();
            if (sheet == null) return null;

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var rows = worksheetPart.Worksheet.Descendants<Row>().ToList();

            // Check dòng đầu tiên có chứa header "ColorName" không
            var headerRow = rows.FirstOrDefault();
            if (headerRow == null || GetCellValue(workbookPart, headerRow.Elements<Cell>().FirstOrDefault()!) != "ColorName")
            {
                return null; // Return null nếu header thiếu header hoặc không chính xác
            }

            // Skip dòng header và xử lí những dòng còn lại
            foreach (var row in rows.Skip(1))
            {
                try
                {
                    var cells = row.Elements<Cell>().ToArray();
                    var color = new ProductColor
                    {
                        ColorName = GetCellValue(workbookPart, cells[0]),
                    };
                    colors.Add(color);
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                    {
                        continue;
                    }
                    throw;
                }
            }
        }

        return colors;
    }

    /// <summary>
    /// Xuất danh sách ProductColor ra file Excel.
    /// </summary>
    /// <param name="filePath">Đường dẫn lưu file Excel.</param>
    /// <param name="colors">Danh sách các đối tượng ProductColor cần xuất.</param>
    public static void ExportColorsToExcel(string filePath, List<ProductColor> colors)
    {
        if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            filePath += ".xlsx";
        }

        using var document = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());

        var sheets = document.WorkbookPart!.Workbook.AppendChild(new Sheets());
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
        sheetData!.Append(headerRow);

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

    /// <summary>
    /// Nhập danh sách đối tượng ProductSize từ file Excel.
    /// </summary>
    /// <param name="filePath">Đường dẫn tới file Excel.</param>
    /// <returns>Danh sách đối tượng ProductSize.</returns>
    public static List<ProductSize>? ImportSizesFromExcel(string filePath)
    {
        var sizes = new List<ProductSize>();

        using (var document = SpreadsheetDocument.Open(filePath, false))
        {
            var workbookPart = document.WorkbookPart;
            var sheet = workbookPart!.Workbook.Sheets!.Elements<Sheet>().FirstOrDefault();
            if (sheet == null) return null;

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var rows = worksheetPart.Worksheet.Descendants<Row>().ToList();

            // Kiểm tra xem dòng đầu tiên có chứa tiêu đề "SizeName" hay không
            var headerRow = rows.FirstOrDefault();
            if (headerRow == null || GetCellValue(workbookPart, headerRow.Elements<Cell>().FirstOrDefault()!) != "SizeName")
            {
                return null; // Trả về null nếu tiêu đề bị thiếu hoặc không chính xác
            }

            // Bỏ qua dòng tiêu đề và xử lý các dòng còn lại
            foreach (var row in rows.Skip(1))
            {
                try
                {
                    var cells = row.Elements<Cell>().ToArray();
                    var size = new ProductSize
                    {
                        SizeName = GetCellValue(workbookPart, cells[0]),
                    };
                    sizes.Add(size);
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                    {
                        continue;
                    }
                    throw;
                }
            }
        }

        return sizes;
    }

    /// <summary>
    /// Xuất danh sách đối tượng ProductSize ra file Excel.
    /// </summary>
    /// <param name="filePath">Đường dẫn lưu file Excel.</param>
    /// <param name="sizes">Danh sách các đối tượng ProductSize cần xuất.</param>
    public static void ExportSizesToExcel(string filePath, List<ProductSize> sizes)
    {
        if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            filePath += ".xlsx";
        }

        using var document = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());

        var sheets = document.WorkbookPart!.Workbook.AppendChild(new Sheets());
        var sheet = new Sheet
        {
            Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
            SheetId = 1,
            Name = "Sizes"
        };
        sheets.Append(sheet);

        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

        // Thêm dòng tiêu đề
        var headerRow = new Row();
        headerRow.Append(
            CreateCell("SizeId"),
            CreateCell("SizeName"),
            CreateCell("IsDeleted")
        );
        sheetData!.Append(headerRow);

        // Thêm các dòng dữ liệu
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

    /// <summary>
    /// Nhập danh sách đối tượng ProductCategory từ file Excel.
    /// </summary>
    /// <param name="filePath">Đường dẫn tới file Excel.</param>
    /// <returns>Danh sách đối tượng ProductCategory.</returns>
    /// <summary>
    /// Nhập danh sách đối tượng ProductCategory từ file Excel.
    /// </summary>
    /// <param name="filePath">Đường dẫn tới file Excel.</param>
    /// <returns>Danh sách đối tượng ProductCategory.</returns>
    public static List<ProductCategory> ImportCategoriesFromExcel(string filePath)
    {
        var categories = new List<ProductCategory>();

        using (var document = SpreadsheetDocument.Open(filePath, false))
        {
            var workbookPart = document.WorkbookPart;
            var sheet = workbookPart!.Workbook.Sheets!.Elements<Sheet>().FirstOrDefault();
            if (sheet == null) return categories;

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var rows = worksheetPart.Worksheet.Descendants<Row>().Skip(1); // Bỏ qua dòng tiêu đề

            foreach (var row in rows)
            {
                try
                {
                    var cells = row.Elements<Cell>().ToArray();
                    var category = new ProductCategory
                    {
                        CategoryName = GetCellValue(workbookPart, cells[0]),
                        ProductType = GetCellValue(workbookPart, cells[1])
                    };
                    categories.Add(category);
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                    {
                        continue;
                    }
                    throw;
                }
            }
        }

        return categories;
    }

    /// <summary>
    /// Xuất danh sách đối tượng ProductCategory ra file Excel.
    /// </summary>
    /// <param name="filePath">Đường dẫn lưu file Excel.</param>
    /// <param name="categories">Danh sách các đối tượng ProductCategory cần xuất.</param>
    public static void ExportCategoriesToExcel(string filePath, List<ProductCategory> categories)
    {
        if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            filePath += ".xlsx";
        }

        using var document = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());

        var sheets = document.WorkbookPart!.Workbook.AppendChild(new Sheets());
        var sheet = new Sheet
        {
            Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
            SheetId = 1,
            Name = "Categories"
        };
        sheets.Append(sheet);

        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

        // Thêm dòng tiêu đề
        var headerRow = new Row();
        headerRow.Append(
            CreateCell("CategoryId"),
            CreateCell("CategoryName"),
            CreateCell("ProductType")
        );
        sheetData!.Append(headerRow);

        // Thêm các dòng dữ liệu
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

    /// <summary>
    /// Nhập danh sách đối tượng Customer từ file Excel.
    /// </summary>
    /// <param name="filePath">Đường dẫn tới file Excel.</param>
    /// <returns>Danh sách đối tượng Customer.</returns>
    public static List<Customer> ImportCustomersFromExcel(string filePath)
    {
        var customers = new List<Customer>();

        using (var document = SpreadsheetDocument.Open(filePath, false))
        {
            var workbookPart = document.WorkbookPart;
            var sheet = workbookPart!.Workbook.Sheets!.Elements<Sheet>().FirstOrDefault();
            if (sheet == null) return customers;

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var rows = worksheetPart.Worksheet.Descendants<Row>().Skip(1); // Bỏ qua dòng tiêu đề

            foreach (var row in rows)
            {
                try
                {
                    var cells = row.Elements<Cell>().ToArray();
                    var customer = new Customer
                    {
                        CustomerName = GetCellValue(workbookPart, cells[0]),
                        Email = GetCellValue(workbookPart, cells[1]),
                        Phone = GetCellValue(workbookPart, cells[2]),
                        Address = GetCellValue(workbookPart, cells[3]),
                        Points = int.TryParse(GetCellValue(workbookPart, cells[4]), out var points) ? points : 0,
                    };
                    customers.Add(customer);
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                    {
                        continue;
                    }
                    throw;
                }
            }
        }

        return customers;
    }

    /// <summary>
    /// Xuất danh sách đối tượng Customer ra file Excel.
    /// </summary>
    /// <param name="filePath">Đường dẫn lưu file Excel.</param>
    /// <param name="customers">Danh sách các đối tượng Customer cần xuất.</param>
    public static void ExportCustomersToExcel(string filePath, List<Customer> customers)
    {
        if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            filePath += ".xlsx";
        }

        using var document = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());

        var sheets = document.WorkbookPart!.Workbook.AppendChild(new Sheets());
        var sheet = new Sheet
        {
            Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
            SheetId = 1,
            Name = "Customers"
        };
        sheets.Append(sheet);

        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

        // Thêm dòng tiêu đề
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
        sheetData!.Append(headerRow);

        // Thêm các dòng dữ liệu
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

    /// <summary>
    /// Nhập danh sách đối tượng Product từ file Excel.
    /// </summary>
    public static List<Product> ImportProductsFromExcel(string filePath)
    {
        var products = new List<Product>();

        using (var document = SpreadsheetDocument.Open(filePath, false))
        {
            var workbookPart = document.WorkbookPart;
            var sheet = workbookPart!.Workbook.Sheets!.Elements<Sheet>().FirstOrDefault();
            if (sheet == null) return products;

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var rows = worksheetPart.Worksheet.Descendants<Row>().Skip(1); // Bỏ qua dòng tiêu đề

            foreach (var row in rows)
            {
                try
                {
                    var cells = row.Elements<Cell>().ToArray();

                    // Phân tích dữ liệu sản phẩm
                    var no = int.TryParse(GetCellValue(workbookPart, cells[0]), out var pid) ? pid : 0;
                    var productName = GetCellValue(workbookPart, cells[1]);
                    var importPrice = int.TryParse(GetCellValue(workbookPart, cells[2]), out var ip) ? ip : 0;
                    var price = int.TryParse(GetCellValue(workbookPart, cells[3]), out var p) ? p : 0;
                    var categoryId = int.TryParse(GetCellValue(workbookPart, cells[4]), out var cid) ? cid : (int?)null;
                    var publicId = GetCellValue(workbookPart, cells[5]);

                    // Kiểm tra xem sản phẩm đã tồn tại chưa
                    var product = products.FirstOrDefault(p => p.ProductName == productName);
                    if (product == null)
                    {
                        product = new Product
                        {
                            ProductName = productName,
                            ImportPrice = importPrice,
                            Price = price,
                            CategoryId = categoryId,
                            PublicId = publicId,
                            ProductVariants = []
                        };
                        products.Add(product);
                    }

                    // Phân tích dữ liệu biến thể sản phẩm
                    var colorId = int.TryParse(GetCellValue(workbookPart, cells[6]), out var colId) ? colId : (int?)null;
                    var sizeId = int.TryParse(GetCellValue(workbookPart, cells[7]), out var szId) ? szId : (int?)null;
                    var stockQuantity = int.TryParse(GetCellValue(workbookPart, cells[8]), out var sq) ? sq : 0;

                    var variant = new ProductVariant
                    {
                        ColorId = colorId,
                        SizeId = sizeId,
                        StockQuantity = stockQuantity,
                    };

                    product.ProductVariants.Add(variant);
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                    {
                        continue;
                    }
                    throw;
                }
            }
        }
        return products;
    }

    /// <summary>
    /// Xuất danh sách đối tượng Product ra file Excel.
    /// </summary>
    public static void ExportProductsToExcel(string filePath, List<Product> products)
    {
        if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            filePath += ".xlsx";
        }

        using var document = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());

        var sheets = document.WorkbookPart!.Workbook.AppendChild(new Sheets());
        var sheet = new Sheet
        {
            Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
            SheetId = 1,
            Name = "Products"
        };
        sheets.Append(sheet);

        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

        // Thêm dòng tiêu đề
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
        sheetData!.Append(headerRow);

        // Thêm các dòng dữ liệu
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

    /// <summary>
    /// Phương thức trợ giúp để lấy giá trị của một ô.
    /// </summary>
    private static string GetCellValue(WorkbookPart workbookPart, Cell cell)
    {
        if (cell == null || cell.CellValue == null) return string.Empty;

        var value = cell.CellValue.Text;
        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        {
            var sharedStringTable = workbookPart.SharedStringTablePart!.SharedStringTable;
            return sharedStringTable.ElementAt(int.Parse(value)).InnerText;
        }

        return value;
    }

    /// <summary>
    /// Phương thức trợ giúp để tạo một ô với giá trị cho trước.
    /// </summary>
    private static Cell CreateCell(string value) => new()
    {
        DataType = CellValues.String,
        CellValue = new CellValue(value)
    };
}
