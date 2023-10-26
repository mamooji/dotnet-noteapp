run:
	@cd WebApi && dotnet run --human-logs

migration:
	@dotnet ef migrations add $(name) --project Infrastructure --startup-project WebApi --output-dir Persistence/Migrations

remove-migration:
	@dotnet ef migrations remove --project Infrastructure --startup-project WebApi

update-database:
	@dotnet ef database update --project Infrastructure --startup-project WebApi

clear-database:
	@cd WebApi && yes | dotnet ef database drop && yes | dotnet ef database update

jump-to-migration:
	@dotnet ef database update $(name) --project Infrastructure --startup-project WebApi

up:
	@docker-compose up -d --build

down:
	@docker-compose down

clean:
	@cd WebApI && dotnet clean

seed:
	@cd WebApi && dotnet run --seed
#	
#clean:
#	@cd src/WebUI && dotnet clean
#
#clear-database:
#	@cd src/WebUI && yes | dotnet ef database drop && yes | dotnet ef database update
#
#test:
#	@dotnet test
#

#up:
#	@docker-compose -f docker-compose.infra.yml up -d --build
#
#up-m1:
#	@docker-compose -f docker-compose.infra.m1.yml up -d --build
#
#down:
#	@docker-compose -f docker-compose.infra.yml down
#
#down-m1:
#	@docker-compose -f docker-compose.infra.m1.yml down
#
#
#build-docker:
#	@docker build -f Dockerfile -t ques:$(cat ./version) .
#
#run-docker:
#	@docker run -it --rm -p 5001:5001 ques:$(cat ./version)
#	
#spidagen:
#	@cd src/Spida && dotnet run 
#
#logs:
#	@docker inspect --format='{{.LogPath}}' $(id)
#	

#
#remove-migration:
#	@dotnet ef migrations remove --project src/Infrastructure --startup-project src/WebUI
#

#
#update-database:
#	@dotnet ef database update --project src/Infrastructure --startup-project src/WebUI
