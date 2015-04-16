/* Copyright (C) 2014 konik.io
 *
 * This file is part of the Konik library.
 *
 * The Konik library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * The Konik library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with the Konik library. If not, see <http://www.gnu.org/licenses/>.
 */
using System;

using java.io;

using org.apache.commons.lang3.time;
using com.neovisionaries.i18n;

using io.konik;
using io.konik.zugferd;
using io.konik.zugferd.entity;
using io.konik.zugferd.entity.trade;
using io.konik.zugferd.entity.trade.item;
using io.konik.zugferd.profile;
using io.konik.zugferd.unece.codes;
using io.konik.zugferd.unqualified;

using Country = com.neovisionaries.i18n.CountryCode;
using Currency = com.neovisionaries.i18n.CurrencyCode;
using io.konik.validation;
using javax.validation;
using java.util;



namespace io.konik.net.example
{
	class NetExampleInvoice
	{
		public static void Main (string[] args)
		{
			System.Console.WriteLine ("Konik for .NET");
			Invoice invoice = createInvoice ();
			validate (invoice);
			attachInvoiceToPdf (invoice);
			System.Console.WriteLine ("ZUGFeRD Invocie created see outputfile acme_invoice-42_ZUGFeRD.pdf");
		}

		public static Invoice createInvoice ()
		{

			ZfDate today = new ZfDateDay ();
			ZfDate nextMonth = new ZfDateMonth (DateUtils.addMonths (today, 1));

			Invoice invoice = new Invoice (ConformanceLevel.BASIC);    // <1>
			invoice.setHeader(new Header()
				.setInvoiceNumber("20131122-42")
				.setCode(DocumentCode._380)
				.setIssued(today)
				.addNote(new Note("MFG"))
				.setName ("Rechnung"));

			Trade trade = new Trade ();
			trade.setAgreement (new Agreement ()    // <2>
				.setSeller (new TradeParty ()
					.setName ("Seller Inc.")
					.setAddress (new Address ("80331", "Marienplatz 1", "München", Country.DE))
					.addTaxRegistrations (new TaxRegistration ("DE122...", Reference.FC)))
				.setBuyer (new TradeParty ()
					.setName ("Buyer Inc.")
					.setAddress (new Address ("50667", "Domkloster 4", "Köln", Country.DE))
					.addTaxRegistrations (new TaxRegistration ("DE123...", Reference.FC))));

			trade.setDelivery (new Delivery (nextMonth));

			trade.setSettlement (new Settlement ()
				.setPaymentReference ("20131122-42")
				.setCurrency (Currency.EUR)
				.addPaymentMeans (new PaymentMeans ()
					.setPayeeAccount (new CreditorFinancialAccount ("DE01234.."))
					.setPayeeInstitution (new FinancialInstitution ("GENO...")))
				.setMonetarySummation (new MonetarySummation ()
					.setLineTotal (new Amount (100, Currency.EUR))
					.setChargeTotal (new Amount (0, Currency.EUR))
					.setAllowanceTotal (new Amount (0, Currency.EUR))
					.setTaxBasisTotal (new Amount (100, Currency.EUR))
					.setTaxTotal (new Amount (19, Currency.EUR))               
					.setGrandTotal (new Amount (119, Currency.EUR))));

			trade.addItem (new Item ()
				.setProduct (new Product ().setName ("Saddle"))
				.setDelivery (new SpecifiedDelivery (new Quantity (1, UnitOfMeasurement.UNIT))));
			invoice.setTrade (trade);

			return invoice;
		}

		static void attachInvoiceToPdf (Invoice invoice)
		{
			PdfHandler handler = new PdfHandler ();    // <1>
			InputStream inputPdf = new FileInputStream ("../../resources/acme_invoice-42.pdf");
			OutputStream resultingPdf = new FileOutputStream ("acme_invoice-42_ZUGFeRD.pdf");
			handler.appendInvoice (invoice, inputPdf, resultingPdf);     // <2>
		}

		static void validate(Invoice invoice){
			
			InvoiceValidator Validator = new InvoiceValidator();
			Set violations = Validator.validate(invoice);  		

			if (violations.size() > 0) {
				System.Console.WriteLine("Invoice has (" + violations.size()  + ") violations.");
				foreach (ConstraintViolation violation in violations.toArray()){
					System.Console.Write (violation.getPropertyPath() + " MSG: ");
					System.Console.Write (violation.getMessage() + "\n");
				}
			} else {
				System.Console.WriteLine ("No Violations in invoice found ");
			}
		}

	}
}
