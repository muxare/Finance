import React from 'react';
import logo from './logo.svg';
import './App.css';
import { getAPI } from './API';
import { CompanyTable } from './Components/CompanyTable';

function App() {
  const [data, setData] = React.useState([]); // set state to hold the result

  //function below triggers the helper function
  const getData = () =>
    getAPI("Companies").then((res) => {
      if (res.status === 200) {
        setData(res.data);
        console.log(data);
      } else {
        console.log(res);
      }
    });

  //this runs the getData trigger function as useEffect
  React.useEffect(() => {
    getData();
  }, []);

  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          Edit <code>src/App.tsx</code> and save to reload.
        </p>
        <a
          className="App-link"
          href="https://reactjs.org"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn React
        </a>
        <CompanyTable Companies={data} />
      </header>
    </div>
  );
}

export default App;
