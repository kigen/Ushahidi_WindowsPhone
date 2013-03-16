using System;
using System.Threading;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Collections.Generic;
using System.Linq;
using Ushahidi.Library.Data;

namespace Ushahidi.Library
{
    public class DataUtil
    {

        //Database!
        private Database db;


        private Mutex dbMutex = new Mutex(false, "DBControl");
        // note: could also have used a lock rather than a mutex.
        // A lock protects against cross-thread access to the data context.
        // The mutex additionally protects against cross-process access.

        //Contructor to initialize the database
        public DataUtil(string dbConnectionString)
        {
            db = new Database(dbConnectionString);
        }

        //Used when the database is modified.
        public void SaveChangesToDB()
        {
            dbMutex.WaitOne();

            try
            {
                // Attempt all updates.
                db.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
            catch (ChangeConflictException e)
            {
                //For debugging.
                System.Diagnostics.Debug.WriteLine("Optimistic concurrency error.");
                System.Diagnostics.Debug.WriteLine(e.Message);
                foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                {
                    MetaTable metatable = db.Mapping.GetTable(occ.Object.GetType());
                    System.Diagnostics.Debug.WriteLine("Table name: {0}", metatable.TableName);
                    foreach (MemberChangeConflict mcc in occ.MemberConflicts)
                    {
                        object currVal = mcc.CurrentValue;
                        object origVal = mcc.OriginalValue;
                        object databaseVal = mcc.DatabaseValue;

                        System.Diagnostics.Debug.WriteLine("current value: {0}", currVal);
                        System.Diagnostics.Debug.WriteLine("original value: {0}", origVal);
                        System.Diagnostics.Debug.WriteLine("database value: {0}", databaseVal);
                    }
                }

                foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                {
                    // Keep the value that has changed, update the other values with database values.
                    occ.Resolve(RefreshMode.KeepChanges);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("ERROR!!!: " + e);
                return;
            }
            // Minimize wait for other threads.
            finally { dbMutex.ReleaseMutex(); }
        }


        //Queries the database and returns the logged locations
        public List<Deployments> getAllDeployments()
        {
            dbMutex.WaitOne();
            List<Deployments> locations = (from q in db.Deployment
                                           select q).ToList<Deployments>();


            dbMutex.ReleaseMutex();
            return locations;

        }


        public List<DeploymentCategory> getDeploymentCategories()
        {
            List<DeploymentCategory> cats = (from q in db.DeploymentCategory
                                            select q).ToList<DeploymentCategory>();

            return cats;
        }

        public void SaveDeploymentCategory(DeploymentCategory dcat)
        {
            if (getDeploymentCategory(dcat.id)==null)
            {
                db.DeploymentCategory.InsertOnSubmit(dcat);
                SaveChangesToDB();
            }
        }


        public void updateAll(){
            SaveChangesToDB();
           
        }


        public void saveDeployment(Deployments deployments)
        {
            if (deployments.isLocal)
            {
                db.Deployment.InsertOnSubmit(deployments);
            }
            else
            {
                if (deployments.name.ToLower() != "ushahidi")
                {
                    Deployments deployment = getDeployment(deployments.id);

                    if (deployment == null)
                    {
                        db.Deployment.InsertOnSubmit(deployments);
                    }
                }
            }

            SaveChangesToDB();
        }

        public void DeleteDeployment(Deployments d)
        {

            dbMutex.WaitOne();

            db.Deployment.DeleteOnSubmit(d);

            SaveChangesToDB();
            dbMutex.ReleaseMutex();


        }


          public DeploymentCategory getDeploymentCategory(string id)
        {
            var deployment = from q in db.DeploymentCategory
                             where q.id == id
                             select q;

            List<DeploymentCategory> dep = deployment.ToList<DeploymentCategory>();
            if (dep.Count == 0)
                return null;
            else
                return dep[0];
           
        }

          public int CategoriesCount()
          {
              var deployment = from q in db.DeploymentCategory                               
                               select q;
              return deployment.Count();
          }


        public Deployments getDeployment(string id)
        {
            var deployment = from q in db.Deployment
                             where q.id == id
                             select q;

            List<Deployments> dep = deployment.ToList<Deployments>();
            if (dep.Count == 0)
                return null;
            else
                return dep[0];
           
        }

    }

}
