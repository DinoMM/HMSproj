Migracie robene na zaklade tutorialov a dopomoc AI a trochu testovania

Ako vytvorit migraciu.

	1. pridaj/zmen/vymaz tabulku, pravidlo ...

	2. otvor Package Manager Console
		Nastav Startup Project na projekt, ktory obsahuje DbContext/Migracie v Solution Explorer
		Nastav Default Project na projekt, ktory obsahuje DbContext/Migracie v Package Manager Console

	3. do Package Manager napis:
	Add-Migration NazovMigracie
	popripade ak mas data v tabulkach a nechce ti prejst: Drop-Database	
	Update-Database



Ako vratit uz updatnutu migraciu.

	1. do Package Manager napis:
	Update-Database NazovMigracieKtoraBolaPridanaCezAddMigrationPredPridanouMigraciou

	2. Remove-Migration

	3. A znova Add-Migration a Update-Database