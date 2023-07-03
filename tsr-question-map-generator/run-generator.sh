#!/bin/bash

POLICIES_URL="https://docs.google.com/spreadsheets/d/e/2PACX-1vT2EEl9ReSBsQp7gErWp7Mfq-Xc41qolOAFjBw6DevQKoLrTh7J2GiB2-OPVGhWG9a80XMGezmncfbU/pub?gid=216103307&single=true&output=csv"
QUESTIONS_URL="https://docs.google.com/spreadsheets/d/e/2PACX-1vT2EEl9ReSBsQp7gErWp7Mfq-Xc41qolOAFjBw6DevQKoLrTh7J2GiB2-OPVGhWG9a80XMGezmncfbU/pub?gid=0&single=true&output=csv"
SECTIONS_URL="https://docs.google.com/spreadsheets/d/e/2PACX-1vT2EEl9ReSBsQp7gErWp7Mfq-Xc41qolOAFjBw6DevQKoLrTh7J2GiB2-OPVGhWG9a80XMGezmncfbU/pub?gid=893394364&single=true&output=csv"
MODULES_URL="https://docs.google.com/spreadsheets/d/e/2PACX-1vT2EEl9ReSBsQp7gErWp7Mfq-Xc41qolOAFjBw6DevQKoLrTh7J2GiB2-OPVGhWG9a80XMGezmncfbU/pub?gid=452999906&single=true&output=csv"

dotnet run --project QuestionMapGen/QuestionMapGen.csproj -- "$POLICIES_URL" "$QUESTIONS_URL" "$SECTIONS_URL" "$MODULES_URL"
