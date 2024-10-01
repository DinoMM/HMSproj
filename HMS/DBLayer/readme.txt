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


Pre buildovania Blazor-servera na dockeri

	1.Zbuildy app
	2.Choj do priecinka kde sa nachadza docker-compose.yml alebo vo vs View->Terminal
	3.Ked tak vymaz container v dockri lebo sa môžu názvy biť
	4.docker-compose up -d       -- spustenie kontainera v detach mode (zbuildi ho pred tym)

	5.docker-compose down		-- zahodenie dockra

	6.docker-compose build --no-cache ossemes		--zrebuildovanie, nie vzdy treba --no-cache
	7.docker-compose up -d --build ossemes			--zrebuildi a spusti, tzv. restart


  Pre build na NGROK
		docker run --net=host -it --name NGROK_web -e NGROK_AUTHTOKEN=24f5uoLyMQkN8XyNqHWGT_7zJUMHf5bVMVEfQwCUL6a ngrok/ngrok:latest http --url=cobra-novel-wolf.ngrok-free.app 8000

Conenction string localhost source
	localhost,1433		--pre connection stirng na local



pre prejdene do command line v dockri do sql servera
	docker exec -it SQL_server bash			
	/opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P 'TaJnEhEsLo!!!123456789' -C



otvor ngrok spojenie pre databazu --(uistit ze je zaznamenany authtoken)
	ngrok tcp 1433     