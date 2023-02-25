using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF.AutomaticViewModels;

namespace Testing
{
    public partial class MainWindow : Window
    {
        public Person MyPerson
        {
            get { return (Person)GetValue(MyPersonProperty); }
            set { SetValue(MyPersonProperty, value); }
        }

        public static readonly DependencyProperty MyPersonProperty =
            DependencyProperty.Register("MyPerson", typeof(Person), typeof(MainWindow), new PropertyMetadata(null));

        public dynamic MyWrappedPerson
        {
            get { return (dynamic)GetValue(MyWrappedPersonProperty); }
            set { SetValue(MyWrappedPersonProperty, value); }
        }

        public static readonly DependencyProperty MyWrappedPersonProperty =
            DependencyProperty.Register("MyWrappedPerson", typeof(AutomaticViewModel), typeof(MainWindow), new PropertyMetadata(null));

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            User tom = new User
            {
                Address = "123 Street St, Street, ST, 12345",
                Age = 22,
                Created = DateTime.Now,
                EmailAddress = "123@123.com",
                Id = 1,
                Name = "Tom"
            };
            User sam = new User
            {
                Address = "1234 Street St, Street, ST, 12345",
                Age = 23,
                Created = DateTime.Now,
                EmailAddress = "1234@1234.com",
                Id = 2,
                Name = "Sam"
            };
            User bob = new User
            {
                Address = "12345 Street St, Street, ST, 12345",
                Age = 24,
                Created = DateTime.Now,
                EmailAddress = "12345@12345.com",
                Id = 3,
                Name = "Bob"
            };

            UserGroup group = new UserGroup
            {
                Id = 1
            };
            group.Users.Add(tom.EmailAddress, tom);
            group.Users.Add(sam.EmailAddress, sam);
            group.Users.Add(bob.EmailAddress, bob);

            AddressBook addressBook = new AddressBook
            {
                CreatedDate = DateTime.Now,
                Name = "My Address Book",
                Owner = tom
            };
            addressBook.RandomNumbersBecause.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            addressBook.Users.Add(tom);
            addressBook.Users.Add(sam);
            addressBook.Users.Add(bob);

            Person person = new Person
            {
                AddressBook = addressBook,
                Id = 1,
                User = tom,
                UserGroup = group
            };

            MyPerson = person;

            AddressBook addressBook2 = new AddressBook
            {
                CreatedDate = DateTime.Now,
                Name = "My Address Book 2",
                Owner = sam
            };
            addressBook2.RandomNumbersBecause.AddRange(new[] { 1, 2, 3, 4, 5 });
            addressBook2.Users.Add(tom);
            addressBook2.Users.Add(bob);

            Person person2 = new Person
            {
                AddressBook = addressBook2,
                Id = 2,
                User = sam,
                UserGroup = group
            };

            MyWrappedPerson = new AutomaticViewModel(person2);

            //AgeGroups ageGroups = new AgeGroups
            //{
            //    Id = 1,
            //    Name = "Age Group 1"
            //};
            //ageGroups.Users.Add(new Tuple<int, List<User>>(20, new List<User> { tom, sam, bob }));
            //ageGroups.Users.Add(new Tuple<int, List<User>>(30, new List<User>()));
        }

        private void ButtonOne_Click(object sender, RoutedEventArgs e)
        {
            // change name of tom programmatically 
            MyPerson.User.Name = "Tom Updated";
        }

        private void ButtonTwo_Click(object sender, RoutedEventArgs e)
        {
            // add a user and a random number to tom's address book
            MyPerson.AddressBook.Users.Add(new User
            {
                Address = "12221",
                Age = 33,
                Created = DateTime.Now,
                EmailAddress = "new@new.com",
                Id = 4,
                Name = "Tom's New"
            });
        }

        private void ButtonThree_Click(object sender, RoutedEventArgs e)
        {
            // change name of sam programmatically 
            MyWrappedPerson.User.Name = "Same Updated";
        }

        private void ButtonFour_Click(object sender, RoutedEventArgs e)
        {
            // add a user and a random number to sam's address book
            MyWrappedPerson.AddressBook.Users.Add(new User
            {
                Address = "32332",
                Age = 37,
                Created = DateTime.Now,
                EmailAddress = "newWrapped@newWrapped.com",
                Id = 4,
                Name = "Sam's New"
            });
        }
    }
}
