using DisSharp;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disbot
{
    static class ItemsDB
    {
        public static async Task<bool> AddItem(string strBuilder)
        {
            try
            {
                var data = strBuilder.Split(' ');
                if (data.Length % 2 != 0)
                    return false;
                var client = new MongoClient(BotConfig.GetContext.MongoDBConnectionString);
                var database = client.GetDatabase("BDO");
                var collection = database.GetCollection<BsonDocument>("Items");
                var item = new BsonDocument();
                item.Add(new BsonElement(data[0], data[1]));
                for (var i = 2; i < data.Length; i += 2)
                {
                    var key = data[i];
                    int.TryParse(data[i + 1], out int value);
                    var elem = new BsonElement(key, value);
                    item.Add(elem);
                }
                await collection.InsertOneAsync(item);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<List<string>> GetItemMaterials(string itemName, int batch)
        {
            var returnValue = new List<string>();
            try
            {
                var client = new MongoClient(BotConfig.GetContext.MongoDBConnectionString);
                var database = client.GetDatabase("BDO");
                var collection = database.GetCollection<BsonDocument>("Items");
                var filter = Builders<BsonDocument>.Filter.Eq("Name", itemName);
                /*
                var document = collection.Find(filter).FirstOrDefault();

                for (var i = 2; i < document.ElementCount; i++)
                {
                    returnValue.Add($@"{document.Names.ToArray()[i]} : {((int)document[i]) * batch}");
                    //Console.WriteLine($@"{document.Names.ToArray()[i]} : {document[i]}");
                }
                */
                var queryResult = new List<BsonDocument>();
                await collection.Find(filter).ForEachAsync(x=> queryResult.Add(x));
                queryResult.ForEach(document => {
                    for (var i = 2; i < document.ElementCount; i++)
                    {
                        returnValue.Add($@"{document.Names.ToArray()[i]} : {((int)document[i]) * batch}");
                    }
                    returnValue.Add("------------------");
                });
       
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                returnValue.Clear();
            }
            return returnValue;
        }
    }
}
