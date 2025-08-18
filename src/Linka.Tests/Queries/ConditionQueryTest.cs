// using NUnit.Framework;
// using Webamoki.Linka.Queries;
// using Webamoki.Utils;
//
// namespace Webamoki.Tests.Core.Data.Queries;
//
// public class ConditionQueryTest
// {
//     [Test]
//     public void Render_EmptyBody_ThrowsException()
//     {
//         var query = new ConditionQuery<Query>();
//         var test = 1;
//         Logging.WriteLog(query.Con(a => test == a.Length));
//
//         // SELECT
//     }
// }
// // SELECT 
// // `Listing`.`ID` as `Listing.ID` ,
// // `Listing`.`Name` as `Listing.Name` ,
// // `Listing`.`Subtitle` as `Listing.Subtitle` ,
// // `Listing`.`URL` as `Listing.URL` ,
// // `Listing`.`Keywords` as `Listing.Keywords` ,
// // `Listing`.`ShortDescription` as `Listing.ShortDescription` ,
// // `Listing`.`State` as `Listing.State` ,
// // `Listing`.`BrandID` as `Listing.BrandID` ,
// // `Listing`.`ManufacturerID` as `Listing.ManufacturerID` ,
// // `Listing`.`StockType` as `Listing.StockType` ,
// // `Brand`.`Name` as `Listing.brand` ,
// // `Brand`.`URL` as `Listing.brandURL` ,
// // `Manu`.`Name` as `Listing.manufacturer`
// // FROM `Listing` LEFT JOIN `Brand` ON `Listing`.`BrandID`=`Brand`.`ID` LEFT JOIN `Brand` AS `Manu` ON `Listing`.`ManufacturerID`=`Manu`.`ID` LIMIT 10