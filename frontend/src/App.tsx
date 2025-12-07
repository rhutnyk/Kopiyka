import './App.css';
import logo from './assets/logo.svg';

function App() {
  return (
    <div className="app">
      <header className="hero">
        <img src={logo} className="logo" alt="Kopiyka logo" />
        <div>
          <p className="eyebrow">Azure Static Web App + .NET backend</p>
          <h1>Kopiyka — family budget hub</h1>
          <p className="lede">
            Fresh project scaffolding for a React front end and Azure Functions backend. No business logic yet, just a
            welcoming starting point.
          </p>
          <div className="cta-row">
            <a className="cta" href="/docs/feature-plan">Explore feature plan</a>
            <a className="secondary" href="/docs/architecture">Read architecture notes</a>
          </div>
        </div>
      </header>
      <section className="panel">
        <h2>Next steps</h2>
        <ul>
          <li>Wire the React app to the Functions API once endpoints are defined.</li>
          <li>Implement auth + data layer aligned with the feature plan.</li>
          <li>Deploy to Azure Static Web Apps to validate the pipeline.</li>
        </ul>
      </section>
    </div>
  );
}

export default App;
