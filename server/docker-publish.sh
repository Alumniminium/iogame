#!/usr/bin/env bash

docker -D build -t herstfortress/iogame:latest . && docker push herstfortress/iogame:latest
