using Mobile.Models;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mobile.Services
{
    public class DatabaseService
    {
        const string DatabaseFilename = "OneMoreTree.db3";

        const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;

        static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }

        readonly SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            _database = new SQLiteAsyncConnection(DatabasePath, Flags);

            // Check if a table already exist
            if (!_database.TableMappings.Any(mapping => mapping.MappedType.Name == typeof(User).Name))
            {
                _database.CreateTablesAsync(CreateFlags.None, typeof(User)).ConfigureAwait(false);
            }

            if (!_database.TableMappings.Any(mapping => mapping.MappedType.Name == typeof(Tree).Name))
            {
                _database.CreateTablesAsync(CreateFlags.None, typeof(Tree)).ConfigureAwait(false);
            }
        }

        public async Task<List<Tree>> GetTreesAsync()
        {
            return await _database.GetAllWithChildrenAsync<Tree>();
        }

        public async Task<Tree> GetTreeAsync(long id)
        {
            return await _database.GetWithChildrenAsync<Tree>(id);
        }

        public async Task SaveTreeAsync(Tree tree)
        {
            // Check if an User has already been loaded on database
            if (await _database.FindAsync<User>(tree.UserId) == null)
            {
                await _database.InsertAsync(tree.User);
            }

            if (await _database.FindAsync<Tree>(tree.Id) != null)
            {
                await _database.UpdateAsync(tree);
            }
            else
            {
                await _database.InsertAsync(tree);
            }
        }

        public async Task DeleteTreeAsync(long id)
        {
            await _database.DeleteAsync<Tree>(id);
        }
    }
}