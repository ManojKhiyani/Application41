# BOS Starter Code for ASP.NET Core 3.1

BOS Starter Code has been designed

- To help engineers easily familiarize themselves with BOS offerings
- To allow you to focus more on the custom logic for the application
- To better handle the data that is shared between BOS and your custom database

## Getting Started

If you are new to the world of .NET Core, start by learning the basics from [Microsoft’s documentation site](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-3.1) and build your first application following the [documentation here.](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc?view=aspnetcore-3.1&tabs=visual-studio)

### PREREQUISITES

The Starter Code is a .NET Core application that uses the MVC pattern.  Before you start coding be sure to have the following appropriate tools installed

#### Development

- IDE
  - [Visual Studio Code](https://code.visualstudio.com/download)
  - [Visual Studio](https://visualstudio.microsoft.com/downloads/)_(doesn't matter the edition)_
- SDK: [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)(_at a minimum_)

#### Database

- If you’ve chosen PostgreSQL in the BOS Console while setting up your application then, we’d recommend you [download it](https://www.postgresql.org/download/), and [pgAdmin](https://www.pgadmin.org/) as the development platform
- If you’ve chosen MySQL, then download the software and [MySQL Workbench](https://dev.mysql.com/downloads/workbench/) as the development tool
- Optional -  [HeidiSQL](https://dev.mysql.com/downloads/workbench/) works with both PostgreSQL and MySQL

### WORKING WITH GITHUB

Based on your selection in the BOS Console, BOS will either create the repo for you in your private GitHub account or in BOS’. Regardless, there are 2 branches where the code is checked-in. One is the `master` and the other is the branch name of your choice. The default name of the second branch is `develop`.

If you already know how to work with GitHub, you can skip this section.

To work with Git, you can either choose to write your commands on the command prompt (Windows), Terminal (Mac), or Gitbash. You can download GitBash from [here](https://git-scm.com/downloads).

#### Cloning your StaterCode

-------------------------------

To start cloning your source code for the Starter Code, you first need to have access to the GitHub repo URL. On BOS Console’s Application Dashboard, look at the **Application Access** tile. Here, you will find the link to the Source Code.

![](https://bos-startercode-uploads.s3.amazonaws.com/01-ApplicationDatabase.png)

The link will take you to GitHub, where when you click on the green Code button, you get your repo URL. Copy this to Clone later.

![](https://bos-startercode-uploads.s3.amazonaws.com/02-GitClone.png)

- Open either command prompt or GitBash
- Change the directory to your desired location
- Clone your source code through the command **git clone _{git URL}_**

```bash
 git clone https://github.com/JamesSmith/James202008242.git
 ```

Note: If you’ve not already logged in, it might ask you to enter your GitHub credentials.

![](https://bos-startercode-uploads.s3.amazonaws.com/03-CommandPropmt-GitHub.png)

#### Pulling the latest

-------------------------------

After cloning the source code to your local computer, you’ll notice that the default branch is set to `master`. The latest version of the source code is already cloned, but to be absolutely sure, we could do this explicitly. _This step is optional._

The link will take you to GitHub, where when you click on the green Code button, you get your repo URL. Copy this to Clone later.

- To pull the latest, do the following
- Navigate to the root folder
- Then execute the command **git pull**

```bash
 git pull
 ```

#### Creating feature branch

-------------------------------

One of the best practices to follow is to create unique feature branches throughout the development cycle. Here we’ll show you how to create a feature branch from the already created `develop` branch.

Execute the following commands

- git checkout develop
- git checkout -b _{FeatureName}_

```bash
git checkout develop
git checkout CustomCode
 ```

#### Pushing/ Committing your code

-------------------------------

Once you’ve done all your changes on your feature branch you could either use VisualStudio to do this or continue using the command prompt. Here is the code snippet to push your code to the feature branch

Execute the following commands

- git commit -am _"{Your message for the commit}"_
- git push

```bash
git commit -am "Added a new message"
git push
 ```

You might encounter an error with the upstream code. To fix this execute the following command

git push --set-upstream original _{YourFeatureBranchName}_

```bash
git push --set-upstream origin CustomCode
 ```

#### Merging your code to the `develop` branch

-------------------------------

Once you feel comfortable with the code in your feature branch and when you think it is deploy-ready, merge your code with the `develop` branch. If you’ve set CI/CD on the BOS Console on your develop (or custom name) branch, then as soon as you merge your code successfully, the CI/ CD process kicks in and the code is deployed to your dev environment. Follow the steps below to merge your code from the feature branch

- git checkout develop
- git merge _{FeatureBranch}_
Example: git merge origin CustomCode
- git checkout develop
- git push

```bash
git checkout develop
git merge origin CustomCode
git checkout develop
git push
 ```

To test if your code has been deployed successfully, visit your application hosted on the development environment.

## Leveraging BOS

BOS Starter Code is already wired with a few of the foundational BOS APIs. Together with this, if you’ve enabled the CI/ CD in the BOS Console, then with every merge to your ‘develop’ (or equivalent) branch the code is deployed to the development server.

A few of the features and functions that the Starter Code already does for you are

- Registration
- Login
- ForgotPassword
- Authorization on login to display only the modules/ features the role is allowed to
- Scoped data per role
- Profile
- For Super Admin _(and based on roles-permissions)_
  - User Management screens
  - Role Management screens
  - Permissions Management screens

To use BOS APIs you’d have to provide the **ClientID and ClientSecret** keys. You can access these in your BOS Console’s Application Dashboard

![](https://bos-startercode-uploads.s3.amazonaws.com/04-APICredentials.png)

### Prewired BOS APIs

The following are the list of BOS foundational APIs that are already integrated into the Starter Code

- User Management and Demographics
- Authentication and Role Management
- Authorization and Information Architecture
- Email

In the source code you’ll find these APIs being used across different controllers. A few of the examples are

- User Management and Demographics - UsersController.cs
- Authentication - AuthController.cs
- Role Management - RolesController.cs
- Authorization - AuthController.cs
- Information Architecture - AuthController.cs, PermissionsController.cs
- Email - UsersController.cs, AuthController.cs

### Configuring Application IA

The Information Architecture in BOS is one of the solid solutions that BOS provides. Information Architecture can be configured in one of two ways.

1. Using BOS Console
    - To set permissions up, navigate to **Application Management** → **Roles and Permissions**
    - Select a **Role** and hit Edit
2. Through the Starter Code
    - Navigate to **Roles** from the left navigation pane and select the **Manage Permissions** operation

BOS readily provides IA for the Super Admin and User roles. However to configure IA for your custom modules (features) in your application, you’ll have to first set this up in the BOS Console. You can create as many Modules (features) and their Operations (actions) in the BOS Console. Speaking MVC language a Module equates to a **Controller** and Operation equates to **Action**.  To do this, navigate to the **Modules and Operations** section under **Application Management**.

## Setting-up Custom Database

In the **appsettings.development.json**, the Starter Code provides the connection string to the development database.

Follow the following steps to set up and use the database locally

- Open the database development tool of your choice.
- Create a new database
- Copy the connection string into clipboard
- In the Source Code, add a new appsetings.Local.json file
- Add the following node in this file

```json
"ConnectionStrings": {
    "StarterCode-MySqlDB": "Server=localhost;User Id=mysql_admin;Password=password;Database=databasename;SearchPath=public"
  }
```

To get information on how you can use Entity Framework Migration in an MVC application, go through the [documentation](https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/intro?tabs=visual-studio&view=aspnetcore-3.1)

## Adding Custom Code

One of the few misconceptions about BOS is that  when you add the Module and its Operations in the BOS Console, it creates the respective controllers in the Starter Code. Well, the BOS team definitely has this in the roadmap, but for right now, the onus is you to manually add them.

For example, here is a screenshot of the custom module that I have created for the application

![](https://bos-startercode-uploads.s3.amazonaws.com/05-CustomModule.png)

The name of the Custom Module that we have the custom code here for is **Tickets** and the Path is **/tickets**. The Super Admin, by default, has access to all the custom modules created, unless explicitly set otherwise.

The module, however, appears on the left navigation of the application when the Super Admin logs in. This is thanks to the BOS IA API. But, when the Super Admin navigates to the “Tickets” module the application navigates to the error page. This is because the Starter Code is yet to be configured to handle this.

![](https://bos-startercode-uploads.s3.amazonaws.com/06-CustomModuleInApp-Tickets.png)

To fix this, we will have to add a new **Controller** to my source code with the page path exactly as mentioned in the BOS Console. So, in this example, I add a new Controller with the name “TicketsController” and also create a default **View** and **Model** to go with it.

### Controller

```cs
 public class TicketsController : Controller
    {
        public IActionResult Index()
        {
            List<Ticket> tickets = new List<Ticket>();
            for (int i = 0; i < 2; i++)
            {
                Ticket ticket = new Ticket();
                ticket.Id = Guid.NewGuid();
                ticket.Number = i;
                ticket.Title = "Ticket " + i;

                tickets.Add(ticket);
            }
            return View(tickets);
        }
    }
```

### Model

```cs
 public class Ticket
    {
        public Guid Id { get; set; }

        public int Number { get; set; }

        public string Title { get; set; }

    }
```

### View

```html
 @model IEnumerable<BOS.StarterCode.Models.BOSModels.Ticket>
    <p>
        <a asp-action="Create">Create New</a>
    </p>
    <table class="table">
        <thead>
            <tr>
                <th>
                    @Html.DisplayNameFor(model => model.Id)
                </th>
                <th>
                    @Html.DisplayNameFor(model => model.Number)
                </th>
                <th>
                    @Html.DisplayNameFor(model => model.Title)
                </th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            @foreach (var item in Model)
            {
                <tr>
                    <td>
                        @Html.DisplayFor(modelItem => item.Id)
                    </td>
                    <td>
                        @Html.DisplayFor(modelItem => item.Number)
                    </td>
                    <td>
                        @Html.DisplayFor(modelItem => item.Title)
                    </td>
                    <td>
                        @Html.ActionLink("Edit", "Edit", new { /* id=item.PrimaryKey */ }) |
                        @Html.ActionLink("Details", "Details", new { /* id=item.PrimaryKey */ }) |
                        @Html.ActionLink("Delete", "Delete", new { /* id=item.PrimaryKey */ })
                    </td>
                </tr>
            }
        </tbody>
    </table>

```

## Working with Google Analytics and Hotjar

BOS Starter Code comes integrated with Google Analytics and Hotjar already, making it extremely simple for engineers to start analysing application/ users behavior. All that you will have to do is include the following code snippet in the appsettings._{environment}_.json file. This allows you to have unique keys for each environment. Look for the following nodes in the appsettings file.

```json
 "Hotjar": "<!-- Hotjar Tracking Code -->\r\n<script>\r\n    (function(h,o,t,j,a,r){\r\n        h.hj=h.hj||function(){(h.hj.q=h.hj.q||[]).push(arguments)};\r\n        h._hjSettings={hjid:XXXXXX,hjsv:6};\r\n        a=o.getElementsByTagName('head')[0];\r\n        r=o.createElement('script');r.async=1;\r\n        r.src=t+h._hjSettings.hjid+j+h._hjSettings.hjsv;\r\n        a.appendChild(r);\r\n    })(window,document,'https://static.hotjar.com/c/hotjar-','.js?sv=');\r\n</script>",
 "GoogleAnalytics": "<!-- Google Analytics Code -->\r\n<script type='text/javascript' async src='https://www.googletagmanager.com/gtag/js?id=XXXXXX'></script>\r\n    <script type='text/javascript'>\r\n        var rightOverlay = null;\r\n        window.dataLayer = window.dataLayer || [];\r\n        function gtag() { dataLayer.push(arguments); }\r\n        gtag('js', new Date());\r\n        var key ='XXXXXX';\r\n        gtag('config', key);\r\n    </script>"
```

Note: Be sure to replace _XXXXXX_ with the appropriate keys.

To know more on how you can create and access your app Keys follow these links along

- [Google Analytics](https://analytics.google.com/analytics/web/provision/#/provision/create)
- [Hotjar](https://www.hotjar.com/)

## Check-ins and Deployment

If you’ve enabled CI/ CD in BOS Console while setting up your application, then everytime there is a code check-in or merge on the selected branch (default is set to `develop`) the code is deployed to the development branch. You will be notified about this via email and also you can look at the build logs on your **Application Dashboard** in BOS Console

## Documentation

To know more about the Business APIs that BOS provides, visit this [website](https://developers.bosframework.com/)