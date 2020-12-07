-- SQL QUERY FOR CREATING covidData TABLE TO STORE ALL DATA FROM https://api.covid19api.com

CREATE TABLE covidData
(
	country NVARCHAR(100) NOT NULL,
	countryCode NVARCHAR(3) NOT NULL,
	province NVARCHAR(100),
	city NVARCHAR(100),
	cityCode NVARCHAR(12),
	lat DECIMAL(9,6),
	lon DECIMAL(9,6),
        confirmed INT NOT NULL DEFAULT(0),
	deaths INT NOT NULL DEFAULT(0),
	recovered INT NOT NULL DEFAULT(0),
	active INT NOT NULL DEFAULT(0),
	caseDate SMALLDATETIME NOT NULL,
	CONSTRAINT PK_covidData PRIMARY KEY (countryCode, caseDate)
);
