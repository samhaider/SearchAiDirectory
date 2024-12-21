namespace SearchAiDirectory.ConsoleApp;

public static class LoadData
{
    public static List<CsvModel> LoadCsv()
    {
        string csvFilePath = "C:\\Users\\Sam\\Downloads\\futuretools2.csv";
        string[] allLines = File.ReadAllLines(csvFilePath);

        if (allLines.Length == 0)
        {
            Console.WriteLine("The CSV file is empty.");
            return null;
        }

        // Extract the header line (the first line)
        string headerLine = allLines[0];
        string[] columns = headerLine.Split(',');

        // This list will hold all the row objects dynamically created
        List<dynamic> dynamicRows = [];
        for (int i = 1; i < allLines.Length; i++)
        {
            string currentLine = allLines[i];
            if (string.IsNullOrWhiteSpace(currentLine)) continue;

            // Split the row by comma
            string[] rowValues = currentLine.Split(',');

            // Create a dynamic object for this row
            dynamic rowObject = new ExpandoObject();
            var rowDictionary = (IDictionary<string, object>)rowObject;

            // Assign each column’s value to the dynamic object
            for (int colIndex = 0; colIndex < columns.Length; colIndex++)
            {
                string columnName = columns[colIndex];
                string columnValue = (colIndex < rowValues.Length) ? rowValues[colIndex] : string.Empty;
                rowDictionary[columnName] = columnValue;
            }

            // Add the dynamic object to the list
            dynamicRows.Add(rowObject);
        }

        List<CsvModel> allRows = [];
        foreach (var exampleRow in dynamicRows)
        {
            var model = new CsvModel
            {
                Name = exampleRow.Col0,
                DetailPage = exampleRow.Col0_HREF,
                Description = exampleRow.Col1,
                Category = exampleRow.Col2,
                LinkPage = exampleRow.Col5_HREF,
                ImageUrl = exampleRow.Col8_SRC
            };

            allRows.Add(model);
        }

        return allRows;
    }

    public class CsvModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string DetailPage { get; set; }
        public string LinkPage { get; set; }
        public string ImageUrl { get; set; }
    }
}