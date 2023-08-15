#This Make file is basically a scripts file:

#starts the app like "npm run dev"
run:
	@cd WebApi && dotnet run 

#seed:
#	@cd src/WebUI && dotnet run --seed
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
#up-app:
#	@docker-compose up -d --build
#
#down-app:
#	@docker-compose down
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
#migration:
#	@dotnet ef migrations add $(name) --project src/Infrastructure --startup-project src/WebUI --output-dir Persistence/Migrations
#
#remove-migration:
#	@dotnet ef migrations remove --project src/Infrastructure --startup-project src/WebUI
#
#jump-to-migration:
#	@dotnet ef database update $(name) --project src/Infrastructure --startup-project src/WebUI
#
#update-database:
#	@dotnet ef database update --project src/Infrastructure --startup-project src/WebUI
