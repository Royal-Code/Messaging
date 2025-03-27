@echo off

git status --porcelain > nul
if errorlevel 1 (
  echo Não há alterações no repositório.
  exit /b 0
)

set /p "commit_message=Digite a mensagem de commit: "

git add --all
git commit -m "%commit_message%"
git push

echo Operações Git concluídas com sucesso.