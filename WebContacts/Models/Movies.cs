using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebContacts.Models
{
    public enum SexType { féminin, masculin }

    public class Movie
    {
        #region Movie fields
        public int Id { get; set; }

        [Display(Name = "Prénom")]
        [RegularExpression(@"^((?!^Name$)[-a-zA-Z0-9àâäçèêëéìîïòôöùûüÿñÀÂÄÇÈÊËÉÌÎÏÒÔÖÙÛÜ_'])+$", ErrorMessage = "Caractères illégaux.")]
        [StringLength(50), Required]
        public String FirstName { get; set; }

        [Display(Name = "Nom")]
        [RegularExpression(@"^((?!^Name$)[-a-zA-Z0-9àâäçèêëéìîïòôöùûüÿñÀÂÄÇÈÊËÉÌÎÏÒÔÖÙÛÜ_'])+$", ErrorMessage = "Caractères illégaux.")]
        [StringLength(50), Required]
        public String LastName { get; set; }

        [Display(Name = "Courriel")]
        [StringLength(255), Required]
        [DataType(DataType.EmailAddress)]
        public String Email { get; set; }

        [Display(Name = "Téléphone")]
        [StringLength(20), Required]
        public String Phone { get; set; }

        [Display(Name = "Avatar")]
        public String AvatarId { get; set; }

        [Display(Name = "Naissance")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime BirthDay { get; set; }

        [Display(Name = "Genre")]
        public SexType Sex { get; set; }
        #endregion

        #region Construction
        public Movie()
        {
            ImageReference = new ImageGUIDReference(@"~/Images/Avatars/", @"Anonymous.png");
            BirthDay = DateTime.Now;
        }

        public Movie Clone()
        {
            Movie clone = new Movie();
            clone.Id = this.Id;
            clone.FirstName = this.FirstName;
            clone.LastName = this.LastName;
            clone.Email = this.Email;
            clone.Phone = this.Phone;
            clone.BirthDay = new DateTime(this.BirthDay.Ticks);
            clone.Sex = this.Sex;
            clone.AvatarId = this.AvatarId;
            return clone;
        }
        #endregion

        #region Avatar handlers
        private ImageGUIDReference ImageReference;

        public String GetAvatarURL()
        {
            return ImageReference.GetImageURL(AvatarId);
        }

        public void UpLoadAvatar(HttpRequestBase Request)
        {
            AvatarId = ImageReference.UpLoadImage(Request, AvatarId);
        }

        public void RemoveAvatar()
        {
            ImageReference.Remove(AvatarId);
        }
        #endregion

        #region File IO handlers

        private const char SEPERATOR = '|';

        public static Movie FromStreamTextLine(String streamTextLine)
        {
            Movie movie = new Movie();
            String[] Tokens = streamTextLine.Split(SEPERATOR);

            movie.Id = int.Parse(Tokens[0]);
            movie.FirstName = Tokens[1];
            movie.LastName = Tokens[2];
            movie.Email = Tokens[3];
            movie.Phone = Tokens[4];
            movie.AvatarId = Tokens[5];
            if (!String.IsNullOrEmpty(Tokens[6]))
                try
                {
                    movie.BirthDay = DateTime.Parse(Tokens[6]);
                }
                catch (Exception e)
                {
                    movie.BirthDay = DateTime.Now;
                }
            else
                movie.BirthDay = DateTime.Now;

            if (!String.IsNullOrEmpty(Tokens[7]))
                try
                {
                    movie.Sex = (SexType)int.Parse(Tokens[7]);
                }
                catch (Exception e)
                {
                    movie.Sex = SexType.masculin;
                }
            else
                movie.Sex = SexType.masculin;

            return movie;
        }

        public String ToStreamTextLine()
        {
            return Id.ToString() + SEPERATOR +
                    FirstName + SEPERATOR +
                    LastName + SEPERATOR +
                    Email + SEPERATOR +
                    Phone + SEPERATOR +
                    AvatarId + SEPERATOR +
                    BirthDay + SEPERATOR +
                    ((int)Sex).ToString() + SEPERATOR;
        }
        #endregion

        #region GetFriends
        public List<Movie> GetFriendsList(Friends Friends, MoviesView contacts)
        {
            List<Movie> ContactFriendsList = new List<Movie>();
            List<Friend> FriendsList = Friends.ToList();

            foreach (Friend Friend in FriendsList)
            {
                if (this.Id == Friend.ContactId)
                    ContactFriendsList.Add(contacts.Get(Friend.FriendId).Clone());
            }

            return ContactFriendsList;
        }

        public List<Movie> GetNotYetFriendsList(Friends Friends, MoviesView contacts)
        {
            List<Movie> ContactNotYetFriendsList = new List<Movie>();
            List<Movie> FriendsList = GetFriendsList(Friends, contacts);
            List<Movie> ContactList = contacts.ToList();

            foreach (Movie movie in ContactList)
            {
                bool isFriend = false;
                foreach(Movie friend in FriendsList)
                {
                    if (movie.Id == friend.Id)
                    {
                        isFriend = true;
                        break;
                    }
                }
                if (!isFriend)
                    ContactNotYetFriendsList.Add(contacts.Get(movie.Id).Clone());
            }

            return ContactNotYetFriendsList;
        }

        public void ClearFriendList(Friends Friends)
        {
            bool checkForMore;
            do
            {
                List<Friend> FriendsList = Friends.ToList();
                checkForMore = false;
                foreach (Friend Friend in FriendsList)
                {
                    if (this.Id == Friend.ContactId)
                    {
                        Friends.Delete(Friend.Id);
                        checkForMore = true;
                        break;
                    }
                }
            } while (checkForMore);
        }
        #endregion
    }

    class ContactComparer : IComparer<Movie>
    {
        #region Sort parameters
        private String sortField = "";
        private bool ascending = true;
        #endregion

        #region Construction
        public ContactComparer(String sortField, bool ascending = true)
        {
            this.sortField = sortField;
            this.ascending = ascending;
        }
        #endregion

        #region Typed comparers
        private int IntCompare(int x, int y)
        {
            if (x > y) return 1;
            if (x < y) return -1;
            return 0;
        }

        private int DateCompare(DateTime x, DateTime y)
        {
            if (x > y) return 1;
            if (x < y) return -1;
            return 0;
        }
        #endregion

        #region Comparer selector
        public int Compare(Movie x, Movie y)
        {
            switch (sortField)
            {
                case "FirstName":
                    if (ascending)
                        return string.Compare(x.FirstName, y.FirstName);
                    else
                        return string.Compare(y.FirstName, x.FirstName);

                case "LastName":
                    if (ascending)
                        return string.Compare(x.LastName, y.LastName);
                    else
                        return string.Compare(y.LastName, x.LastName);

                case "Email":
                    if (ascending)
                        return string.Compare(x.Email, y.Email);
                    else
                        return string.Compare(y.Email, x.Email);
                case "Phone":
                    if (ascending)
                        return string.Compare(x.Phone, y.Phone);
                    else
                        return string.Compare(y.Phone, x.Phone);

                case "BirthDay":
                    if (ascending)
                        return DateCompare(x.BirthDay, y.BirthDay);
                    else
                        return DateCompare(y.BirthDay, x.BirthDay);

                case "Sex":
                    if (ascending)
                        return IntCompare((int)x.Sex, (int)y.Sex);
                    else
                        return IntCompare((int)y.Sex, (int)x.Sex);
            }

            return 0;
        }
        #endregion
    }
    
    public class Movies
    {
        public DateTime LastUpdate { get { return _LastUpdate; } }

        #region Construction
        public Movies(String filePath)
        {
            FilePath = filePath;
            Read();
        }
        #endregion

        #region Locking handlers
        private bool locked = false;

        private void WaitForUnlocked()
        {
            while (locked) { /* do nothing */}
        }

        private void Lock()
        {
            WaitForUnlocked();
            locked = true;
        }

        private void UnLock()
        {
            locked = false;
        }
        #endregion

        #region File IO handlers

        String FilePath;
        private DateTime _LastUpdate = DateTime.Now;

        private void Read()
        {
            WaitForUnlocked();
            StreamReader sr = new StreamReader(FilePath, Encoding.Unicode);

            List.Clear();
            while (!sr.EndOfStream)
            {
                List.Add(Movie.FromStreamTextLine(sr.ReadLine()));
            }
            sr.Close();
            _LastUpdate = DateTime.Now;
        }

        private void Save()
        {
            StreamWriter sw = new StreamWriter(FilePath, false /*erase*/, Encoding.Unicode);

            foreach (Movie contact in List)
            {
                sw.WriteLine(contact.ToStreamTextLine());
            }
            sw.Close();
            _LastUpdate = DateTime.Now;
        }
        #endregion

        #region Contact list handlers

        private List<Movie> List = new List<Movie>();

        public List<Movie> ToList()
        {
            WaitForUnlocked();
            return List;
        }

        public List<Movie> CloneList()
        {
            WaitForUnlocked();
            List<Movie> clone = new List<Movie>();
            foreach (Movie contact in List)
            {
                clone.Add(contact.Clone());
            }
            return clone;
        }

        #endregion

        #region CRUD queries
        public int Add(Movie contact)
        {
            Lock();
            int maxId = 0;

            foreach (Movie c in List)
            {
                if (c.Id > maxId)
                    maxId = c.Id;
            }
            contact.Id = maxId + 1;
            List.Add(contact);
            Save();
            UnLock();
            return contact.Id;
        }

        public void Delete(String id)
        {
            Delete(int.Parse(id));
        }

        public void Delete(int Id)
        {
            Lock();
            int index = -1;
            for (int i = 0; i < List.Count; i++)
            {
                if (List[i].Id == Id)
                    index = i;
            }
            if (index > -1)
            {
                List[index].RemoveAvatar();
                List.RemoveAt(index);
                Save();
            }
            UnLock();
        }

        public void Update(Movie contact)
        {
            Lock();
            int index = -1;
            for (int i = 0; i < List.Count; i++)
            {
                if (List[i].Id == contact.Id)
                    index = i;
            }
            if (index > -1)
            {
                List[index].FirstName = contact.FirstName;
                List[index].LastName = contact.LastName;
                List[index].Email = contact.Email;
                List[index].Phone = contact.Phone;
                List[index].AvatarId = contact.AvatarId;
                List[index].BirthDay = new DateTime(contact.BirthDay.Ticks);
                List[index].Sex = contact.Sex;

                Save();
            }
            UnLock();
        }

        public Movie Get(int Id)
        {
            Lock();
            for (int i = 0; i < List.Count; i++)
            {
                if (List[i].Id == Id)
                {
                    UnLock();
                    return List[i];
                }
            }
            UnLock();
            return null;
        }

        #endregion
    }

    public class MoviesView
    {
        private List<Movie> List = new List<Movie>();
        private DateTime LastUpdate = new DateTime(0);
        private bool ascending = true;
        private String sortField = "";
        private Movies contacts = null;
        private bool toggleSortDirection = true;

        public MoviesView(Movies contacts, String FieldToSort = "")
        {
            this.contacts = contacts;
            sortField = FieldToSort;
            UpdateList();
        }

        public List<Movie> ToList()
        {
            UpdateList();
            return List;
        }

        private void UpdateList()
        {
            if (contacts.LastUpdate > LastUpdate)
            {
                List.Clear();
                List = contacts.CloneList();
                LastUpdate = contacts.LastUpdate;
                toggleSortDirection = false;
                Sort(sortField);
            }
        }

        public void Sort(String FieldToSort = "")
        {
            UpdateList();
            if (!String.IsNullOrEmpty(FieldToSort))
            {
                if (sortField != FieldToSort)
                {
                    sortField = FieldToSort;
                    ascending = true;
                    toggleSortDirection = true;
                }
                else
                {
                    if (toggleSortDirection)
                        ascending = !ascending;
                    toggleSortDirection = true;
                }
                List.Sort(new ContactComparer(sortField, ascending));
            }
        }
        public Movie Get(int Id)
        {
            for (int i = 0; i < List.Count; i++)
            {
                if (List[i].Id == Id)
                {
                    return List[i];
                }
            }
            return null;
        }
    }

    public class Friend
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public int FriendId { get; set; }

        public Friend()
        {
            Id = 0;
            ContactId = 0;
            FriendId = 0;
        }

        public Friend(int ContactId, int FriendId)
        {
            Id = 0;
            this.ContactId = ContactId;
            this.FriendId = FriendId;
        }

        public Friend Clone()
        {
            Friend clone = new Friend();
            clone.Id = this.Id;
            clone.ContactId = this.ContactId;
            clone.FriendId = this.FriendId;
            return clone;
        }

        private const char SEPERATOR = '|';

        public static Friend FromStreamTextLine(String streamTextLine)
        {
            Friend Friend = new Friend();
            String[] Tokens = streamTextLine.Split(SEPERATOR);

            Friend.Id = int.Parse(Tokens[0]);
            Friend.ContactId = int.Parse(Tokens[1]);
            Friend.FriendId = int.Parse(Tokens[2]);

            return Friend;
        }

        public String ToStreamTextLine()
        {
            return  Id.ToString() + SEPERATOR +
                    ContactId.ToString() + SEPERATOR +
                    FriendId.ToString() + SEPERATOR;
        }
    }

    public class Friends
    {
        public DateTime LastUpdate { get { return _LastUpdate; } }

        #region Construction
        public Friends(String filePath)
        {
            FilePath = filePath;
            Read();
        }
        #endregion

        #region Locking handlers
        private bool locked = false;

        private void WaitForUnlocked()
        {
            while (locked) { /* do nothing */}
        }

        private void Lock()
        {
            WaitForUnlocked();
            locked = true;
        }

        private void UnLock()
        {
            locked = false;
        }
        #endregion

        #region File IO handlers

        String FilePath;
        private DateTime _LastUpdate = DateTime.Now;

        private void Read()
        {
            WaitForUnlocked();
            StreamReader sr = new StreamReader(FilePath, Encoding.Unicode);

            List.Clear();
            while (!sr.EndOfStream)
            {
                List.Add(Friend.FromStreamTextLine(sr.ReadLine()));
            }
            sr.Close();
            _LastUpdate = DateTime.Now;
        }

        private void Save()
        {
            StreamWriter sw = new StreamWriter(FilePath, false /*erase*/, Encoding.Unicode);

            foreach (Friend Friend in List)
            {
                sw.WriteLine(Friend.ToStreamTextLine());
            }
            sw.Close();
            _LastUpdate = DateTime.Now;
        }
        #endregion

        #region Friends list handlers
        private List<Friend> List = new List<Friend>();

        public List<Friend> ToList()
        {
            WaitForUnlocked();
            return List;
        }

        public List<Friend> CloneList()
        {
            WaitForUnlocked();
            List<Friend> clone = new List<Friend>();
            foreach (Friend Friend in List)
            {
                clone.Add(Friend.Clone());
            }
            return clone;
        }
        #endregion

        #region CRUD queries
        public void Add(Friend Friend)
        {
            Lock();
            int maxId = 0;

            foreach (Friend f in List)
            {
                if (f.Id > maxId)
                    maxId = f.Id;
            }
            Friend.Id = maxId + 1;
            List.Add(Friend);
            Save();
            UnLock();
        }

        public void Delete(String id)
        {
            Delete(int.Parse(id));
        }

        public void Delete(int Id)
        {
            Lock();
            int index = -1;
            for (int i = 0; i < List.Count; i++)
            {
                if (List[i].Id == Id)
                    index = i;
            }
            if (index > -1)
            {
                List.RemoveAt(index);
                Save();
            }
            UnLock();
        }

        public Friend Get(int Id)
        {
            Lock();
            for (int i = 0; i < List.Count; i++)
            {
                if (List[i].Id == Id)
                {
                    UnLock();
                    return List[i];
                }
            }
            UnLock();
            return null;
        }

        #endregion
    }

}

