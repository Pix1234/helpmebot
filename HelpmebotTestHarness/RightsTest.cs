﻿using helpmebot6.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using helpmebot6;

namespace HelpmebotTestHarness
{
    
    
    /// <summary>
    ///This is a test class for RightsTest and is intended
    ///to contain all RightsTest Unit Tests
    ///</summary>
    [TestClass( )]
    public class RightsTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for getRights
        ///</summary>
        [TestMethod( )]
        public void getRightsTest( )
        {
            Rights target = new Rights( ); // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.getRights( username );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for execute
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        public void executeTest( )
        {
            Rights_Accessor target = new Rights_Accessor( ); // TODO: Initialize to an appropriate value
            User source = null; // TODO: Initialize to an appropriate value
            string channel = string.Empty; // TODO: Initialize to an appropriate value
            string[ ] args = null; // TODO: Initialize to an appropriate value
            CommandResponseHandler_Accessor expected = null; // TODO: Initialize to an appropriate value
            CommandResponseHandler_Accessor actual;
            actual = target.execute( source, channel, args );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for Rights Constructor
        ///</summary>
        [TestMethod( )]
        public void RightsConstructorTest( )
        {
            Rights target = new Rights( );
            Assert.Inconclusive( "TODO: Implement code to verify target" );
        }
    }
}