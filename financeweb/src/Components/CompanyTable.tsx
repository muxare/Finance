import React from "react";
import { Company } from "../Models/Company";

export interface CompanyTableProps {
    Companies: Company[]
}

export const CompanyTable: React.FC<CompanyTableProps>  = (props: CompanyTableProps) => {
    const data = props.Companies;
    return (
        <div>
            <table>
                <tr>
                        <td>Name</td>
                        <td>Ticker</td>
                        <td>Url</td>
                        <td>RowKey</td>
                </tr>
                {data.map(c => (
                    <tr key={c.RowKey}>
                        <td>{c.Name}</td>
                        <td>{c.Ticker}</td>
                        <td>{c.Url}</td>
                        <td>{c.RowKey}</td>
                    </tr>
                ))}
            </table>
        </div>
    )
}