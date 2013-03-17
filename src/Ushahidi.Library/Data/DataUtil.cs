using System;
using System.Threading;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace Ushahidi.Library.Data
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


        /// <summary>
        /// Get a list of all deployments
        /// </summary>
        /// <returns></returns>
        public List<Deployments> getAllDeployments()
        {
            dbMutex.WaitOne();
            List<Deployments> locations = (from q in db.Deployment
                                           select q).ToList<Deployments>();


            dbMutex.ReleaseMutex();
            return locations;

        }

        /// <summary>
        /// Retrieve a list of DeploymentCategories
        /// </summary>
        /// <returns></returns>
        public List<DeploymentCategory> getDeploymentCategories()
        {
            List<DeploymentCategory> cats = (from q in db.DeploymentCategory
                                            select q).ToList<DeploymentCategory>();

            return cats;
        }

        /// <summary>
        /// Save a DeploymentCategory
        /// </summary>
        /// <param name="dcat"></param>
        public void SaveDeploymentCategory(DeploymentCategory dcat)
        {
            if (getDeploymentCategory(dcat.id)==null)
            {
                db.DeploymentCategory.InsertOnSubmit(dcat);
                SaveChangesToDB();
            }
        }

        /// <summary>
        /// Enforce saving of all updates 
        /// </summary>
        public void updateAll(){
            SaveChangesToDB();
           
        }

        /// <summary>
        /// Save a Deployment record
        /// </summary>
        /// <param name="deployments"></param>
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

        /// <summary>
        /// Delete a single deployment record 
        /// </summary>
        /// <param name="d"></param>
        public void DeleteDeployment(Deployments d)
        {

            dbMutex.WaitOne();

            db.Deployment.DeleteOnSubmit(d);

            SaveChangesToDB();
            dbMutex.ReleaseMutex();


        }

        /// <summary>
        /// Retrive a DeploymentCategory by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Retrieve count of categories 
        /// </summary>
        /// <returns></returns>
          public int CategoriesCount()
          {
              var deployment = from q in db.DeploymentCategory                               
                               select q;
              return deployment.Count();
          }

        /// <summary>
        /// Retrieve a Deployment by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
